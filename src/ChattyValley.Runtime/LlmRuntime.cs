using System.Runtime.CompilerServices;
using LLama;
using LLama.Common;
using LLama.Native;
using LLama.Sampling;

namespace ChattyValley.Runtime;

/// <summary>
/// Thin LLamaSharp wrapper: load a base GGUF in-process and stream a reply, optionally with a
/// per-villager LoRA applied (the shared-base + per-villager adapter design, plan doc 06). One base
/// serves every character; <see cref="SetActiveAdapter"/> is how you swap villagers.
///
/// Shared by both the standalone harness (net10.0) and the SMAPI mod (net6.0). LLamaSharp 0.27.0 has
/// no load-time LoRA on ModelParams, and StatelessExecutor builds a fresh context per call, so the
/// adapter is applied to that fresh context (llama_set_adapter_lora, via SetLoraAdapters) before the
/// prompt is decoded. The GGUF adapter is loaded once against the model and reused across turns.
/// </summary>
public sealed class LlmRuntime : IAsyncDisposable
{
    private LLamaWeights? _weights;
    private ModelParams? _params;
    private readonly Dictionary<string, string> _adapters = new();      // name -> gguf path
    private readonly Dictionary<string, LoraAdapter> _loaded = new();   // name -> loaded handle
    private LoraAdapter? _activeAdapter;
    private float _activeScale = 1.0f;

    public bool IsReady => _weights is not null;

    public async Task<TimeSpan> LoadBaseAsync(string modelPath, int contextSize = 2048, int gpuLayers = 0)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _params = new ModelParams(modelPath)
        {
            ContextSize = (uint)contextSize,
            GpuLayerCount = gpuLayers,
        };
        _weights = await LLamaWeights.LoadFromFileAsync(_params);
        sw.Stop();
        return sw.Elapsed;
    }

    /// <summary>Register a per-villager LoRA GGUF under a name (does not load it yet).</summary>
    public void RegisterAdapter(string name, string path) => _adapters[name] = path;

    /// <summary>
    /// Make a registered adapter active for subsequent inferences, or pass null to run the stock
    /// base. The GGUF is loaded against the model on first activation and cached.
    /// </summary>
    public void SetActiveAdapter(string? name, float scale = 1.0f)
    {
        if (name is null) { _activeAdapter = null; return; }
        if (!_adapters.TryGetValue(name, out var path))
            throw new ArgumentException($"adapter '{name}' is not registered", nameof(name));
        if (!_loaded.TryGetValue(name, out var adapter))
            _loaded[name] = adapter = _weights!.NativeHandle.LoadLoraFromFile(path);
        _activeAdapter = adapter;
        _activeScale = scale;
    }

    public async IAsyncEnumerable<string> InferStreamAsync(
        string prompt, float temperature, int maxTokens,
        float repeatPenalty = 1.1f, float frequencyPenalty = 0.1f,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var inferenceParams = new InferenceParams
        {
            MaxTokens = maxTokens,
            // "\n\n" cuts a small model off before it runs into narrating a second paragraph or the
            // other speaker's turn; the ChatML markers bound the assistant turn.
            AntiPrompts = new List<string> { "<|im_end|>", "<|im_start|>", "\n\n" },
            // DefaultSamplingPipeline ships with RepeatPenalty = 1 (i.e. OFF), and a 350M model with a
            // strongly-fit character LoRA will loop a favourite token ("once once once...") several
            // turns into a conversation without it. The penalty ring buffer only sees tokens generated
            // in THIS call (prompt tokens are never accepted into the chain), so PenaltyCount 128
            // comfortably covers a full 96-token reply.
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = temperature,
                RepeatPenalty = repeatPenalty,
                FrequencyPenalty = frequencyPenalty,
                PenaltyCount = 128,
            },
        };

        if (_activeAdapter is { } active)
        {
            // Per-villager LoRA on a fresh (stateless) context, applied before the prompt is decoded.
            using var context = _weights!.CreateContext(_params!);
            context.NativeHandle.SetLoraAdapters(new[] { (active, _activeScale) });
            var executor = new InteractiveExecutor(context);
            await foreach (var token in executor.InferAsync(prompt, inferenceParams, ct))
                yield return token;
        }
        else
        {
            // Stock base (Stage 1a): the proven prompt-only path.
            var executor = new StatelessExecutor(_weights!, _params!);
            await foreach (var token in executor.InferAsync(prompt, inferenceParams, ct))
                yield return token;
        }
    }

    public ValueTask DisposeAsync()
    {
        foreach (var a in _loaded.Values) a.Unload();
        _weights?.Dispose();
        return ValueTask.CompletedTask;
    }
}
