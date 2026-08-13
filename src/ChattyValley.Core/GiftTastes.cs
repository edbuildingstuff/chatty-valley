namespace ChattyValley.Core;

/// <summary>
/// Stardew's gift-taste codes translated into the word the prompt uses.
///
/// The game returns an int from <c>NPC.getGiftTasteForThisItem</c>. Those constants are stable
/// content values, so they live here rather than in the mod project, which cannot be unit tested
/// behind SMAPI. The strings match what <see cref="PromptBuilder"/> renders and what the training
/// data was authored against.
/// </summary>
public static class GiftTastes
{
    // NPC.gift_taste_* in the game. Odd values exist for the "stardrop" and universal cases and
    // are deliberately not mapped: anything unrecognised yields null and the clause omits the taste.
    public const int Love = 0;
    public const int Like = 2;
    public const int Dislike = 4;
    public const int Hate = 6;
    public const int Neutral = 8;

    /// <summary>The taste word for a game taste code, or null when the code is not one we render.</summary>
    public static string? Describe(int tasteCode) => tasteCode switch
    {
        Love => "love",
        Like => "like",
        Dislike => "dislike",
        Hate => "hate",
        Neutral => "neutral",
        _ => null,
    };
}
