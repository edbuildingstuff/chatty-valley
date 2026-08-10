using ChattyValley.Core;

namespace ChattyValley.Tests;

/// <summary>
/// Replies are displayed through Stardew's own Dialogue parser (vanilla DialogueBox UI), so model
/// text must never carry the parser's control characters, and the "@" placeholder is substituted
/// by us, deterministically, before the game sees the string.
/// </summary>
public class DialogueTextSanitizerTests
{
    [Fact]
    public void PlaceholderBecomesPlayerName() =>
        Assert.Equal("And I you, Edward. Welcome back.",
            DialogueTextSanitizer.Sanitize("And I you, @. Welcome back.", "Edward"));

    // The two placeholder positions the A/B-era harness transcripts actually produced (DAT-743):
    // a greeting-opener "@" and a mid-sentence "@" in the same reply. Transcripts keep "@"
    // canonical by design; this pins that the display path substitutes every occurrence.
    [Fact]
    public void EveryPlaceholderOccurrenceIsSubstituted() =>
        Assert.Equal("Ah, hello Edward. Go well, Edward; the mountain keeps its quiet.",
            DialogueTextSanitizer.Sanitize("Ah, hello @. Go well, @; the mountain keeps its quiet.", "Edward"));

    [Fact]
    public void BoxSplitHashBecomesPause() =>
        Assert.Equal("One thing, another thing.",
            DialogueTextSanitizer.Sanitize("One thing# another thing.", "Edward"));

    [Fact]
    public void GenderSwitchCaretBecomesSpace() =>
        Assert.Equal("sir madam",
            DialogueTextSanitizer.Sanitize("sir^madam", "Edward"));

    [Fact]
    public void EmotionCodeDollarIsStripped() =>
        Assert.Equal("Hello there h!",
            DialogueTextSanitizer.Sanitize("Hello there $h!", "Edward"));

    [Fact]
    public void LoneDollarSurvives() =>
        Assert.Equal("It costs 5 dollars, or $ 5.",
            DialogueTextSanitizer.Sanitize("It costs 5 dollars, or $ 5.", "Edward"));

    [Fact]
    public void PlainProsePassesThroughUntouched() =>
        Assert.Equal("The mist is lifting off the lake already; come see.",
            DialogueTextSanitizer.Sanitize("The mist is lifting off the lake already; come see.", "Edward"));
}
