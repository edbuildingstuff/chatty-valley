namespace ChattyValley.Core;

/// <summary>
/// Externalised chat template, an idea taken from reading ValleyTalk: swap base models by swapping
/// this template string, not the code. Placeholders: {system}, {prompt}.
/// </summary>
public sealed class ChatTemplate
{
    public string System { get; init; } = "{system}";
    public string Prompt { get; init; } = "{prompt}";
    public string ResponseStart { get; init; } = "";
    /// <summary>A completed assistant turn in the history (its full text plus the end marker).</summary>
    public string AssistantTurn { get; init; } = "{response}";

    /// <summary>ChatML, the format Qwen2.5 and many small instruct models use.</summary>
    public static ChatTemplate ChatML => new()
    {
        System = "<|im_start|>system\n{system}<|im_end|>\n",
        Prompt = "<|im_start|>user\n{prompt}<|im_end|>\n",
        ResponseStart = "<|im_start|>assistant\n",
        AssistantTurn = "<|im_start|>assistant\n{response}<|im_end|>\n",
    };

    /// <summary>
    /// LFM2 / LFM2.5 (LiquidAI), the base family this project fine-tunes.
    /// It shares ChatML control tokens (im_start / im_end) and a system role; the BOS token
    /// &lt;|startoftext|&gt; is added automatically by llama.cpp (add_bos_token = true), so it must
    /// NOT be written here or it would be duplicated.
    /// </summary>
    public static ChatTemplate Lfm2 => new()
    {
        System = "<|im_start|>system\n{system}<|im_end|>\n",
        Prompt = "<|im_start|>user\n{prompt}<|im_end|>\n",
        ResponseStart = "<|im_start|>assistant\n",
        AssistantTurn = "<|im_start|>assistant\n{response}<|im_end|>\n",
    };

    public string Render(string system, string prompt) =>
        System.Replace("{system}", system)
        + Prompt.Replace("{prompt}", prompt)
        + ResponseStart;
}
