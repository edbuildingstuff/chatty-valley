namespace ChattyValley.Core;

/// <summary>
/// A villager persona. In Stage 1a the voice comes from <see cref="Bio"/> + <see cref="FewShot"/>
/// on a stock model. In Stage 1b it comes from <see cref="AdapterPath"/> (a per-villager LoRA),
/// and the few-shot anchors drop away because the voice then lives in the weights, not the prompt.
/// The <see cref="AdapterPath"/> field is the seam that lets one shared base serve many characters.
/// </summary>
public sealed class Character
{
    public string Name { get; init; } = "";
    public string Bio { get; init; } = "";
    public List<string> FewShot { get; init; } = new();
    public string? AdapterPath { get; init; }  // null = prompt-only (Stage 1a); set = per-villager LoRA (Stage 1b)
}
