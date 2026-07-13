using System.Runtime.CompilerServices;
using LLama;
using LLama.Common;
using LLama.Sampling;

namespace ChattyValley.Harness;

/// <summary>
/// Thin LLamaSharp wrapper: load a base GGUF in-process and stream a reply. The adapter hooks are
/// Stage 1b scaffolding (one shared base + per-villager LoRA, plan doc 06) so growing from one
/// villager to many is a config extension, not a rewrite. With no adapter trained yet they are
/// inert; the real llama.cpp adapter_lora_init / set / clear calls slot in behind the same methods.
/// </summary>
public sealed class LlmRuntime : IAsyncDisposable
{
    private LLamaWeights? _weights;
    private ModelParams? _params;
    private readonly Dictionary<string, string> _adapters = new();

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

    // Stage 1b scaffolding. Kept so the mod's runtime shares this shape.
    public void RegisterAdapter(string name, string path) => _adapters[name] = path;
    public void SetActiveAdapter(string? name) { /* TODO Stage 1b: clear + apply the LoRA on the context */ }

    public async IAsyncEnumerable<string> InferStreamAsync(
        string prompt, float temperature, int maxTokens,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var executor = new StatelessExecutor(_weights!, _params!);
        var inferenceParams = new InferenceParams
        {
            MaxTokens = maxTokens,
            // "\n\n" cuts a small model off before it runs into narrating a second paragraph or the
            // other speaker's turn; the ChatML markers bound the assistant turn.
            AntiPrompts = new List<string> { "<|im_end|>", "<|im_start|>", "\n\n" },
            SamplingPipeline = new DefaultSamplingPipeline { Temperature = temperature },
        };

        await foreach (var token in executor.InferAsync(prompt, inferenceParams, ct))
            yield return token;
    }

    public ValueTask DisposeAsync()
    {
        _weights?.Dispose();
        return ValueTask.CompletedTask;
    }
}
