using System.Text.Json;
using ChattyValley.Core;

namespace ChattyValley.Tests;

/// <summary>
/// The false-premise detector exists twice: <see cref="ConversationSignals"/> here, which is what
/// the mod actually injects the guard from, and eval_linus.FALSE_PREMISE in the gtm training tree,
/// which is what every stage-5 instrument measures against. They are hand-kept ports of one
/// alternation, so drift survives a green build. It leaves the eval scoring a guard the player
/// never gets, and the numbers still look fine.
///
/// Both sides assert against fixtures/false-premise-cases.json. Regenerate it from the gtm side
/// after any detector change (characters/verify_packs.py prints the command) and run both suites.
/// </summary>
public class FalsePremiseParityTests
{
    private sealed record Case(string Message, bool Expect, string Source);

    private static IReadOnlyList<Case> LoadCases()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "fixtures", "false-premise-cases.json");
        Assert.True(File.Exists(path), $"parity fixture missing at {path}");

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var cases = doc.RootElement.GetProperty("cases").EnumerateArray()
            .Select(e => new Case(
                e.GetProperty("message").GetString()!,
                e.GetProperty("expect").GetBoolean(),
                e.GetProperty("source").GetString()!))
            .ToList();

        Assert.NotEmpty(cases);
        return cases;
    }

    [Fact]
    public void DetectorMatchesTheSharedFixture()
    {
        var mismatches = LoadCases()
            .Where(c => ConversationSignals.LooksLikeFalsePremise(c.Message) != c.Expect)
            .Select(c => $"  [{c.Source}] expected fire={c.Expect}, got {!c.Expect}: {c.Message}")
            .ToList();

        Assert.True(mismatches.Count == 0,
            "the C# detector has drifted from the Python one used by the eval instruments:\n"
            + string.Join("\n", mismatches));
    }

    [Fact]
    public void FixtureCoversBothOutcomes()
    {
        // A fixture that only carries firing cases would pass a detector that matches everything,
        // so the precision half has to be present for the parity check to mean anything.
        var cases = LoadCases();
        Assert.Contains(cases, c => c.Expect);
        Assert.Contains(cases, c => !c.Expect);
    }

    [Theory]
    // The three shapes DAT-755 added, kept as named tests so a future trim of the alternation
    // fails with the reason rather than with a fixture line number.
    [InlineData("Everyone knows you taught at the school in Zuzu City before you came here.")]
    [InlineData("Tell me about the gull that waits on your roof.")]
    [InlineData("How's the crab you keep in your pocket doing?")]
    public void CompanionAndPastProfessionShapesFire(string message) =>
        Assert.True(ConversationSignals.LooksLikeFalsePremise(message), message);

    [Theory]
    // Precision is the detector's stated contract: a false fire nags during ordinary talk.
    [InlineData("Do you know Robin?")]
    [InlineData("Are you close with Leah?")]
    [InlineData("Is there a bird that keeps you company?")]
    [InlineData("What did you do today?")]
    public void OrdinaryTalkDoesNotFire(string message) =>
        Assert.False(ConversationSignals.LooksLikeFalsePremise(message), message);
}
