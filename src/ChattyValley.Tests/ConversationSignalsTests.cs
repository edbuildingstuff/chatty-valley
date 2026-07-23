using ChattyValley.Core;

namespace ChattyValley.Tests;

/// <summary>
/// The hybrid end-of-conversation contract: a conservative runtime heuristic catches explicit
/// player farewells today; the trained [end] marker (v3+ adapters, data/linus/batches/farewell.md)
/// covers the messy cases. False positives close a conversation the player wanted to continue, so
/// the heuristic errs toward NOT matching.
/// </summary>
public class ConversationSignalsTests
{
    // ---- player farewell heuristic ----------------------------------------------------------

    [Theory]
    [InlineData("bye")]
    [InlineData("Bye!")]
    [InlineData("bye Linus")]
    [InlineData("goodbye")]
    [InlineData("Good bye, Linus.")]
    [InlineData("gotta go")]
    [InlineData("I gotta run, see you")]
    [InlineData("gtg")]
    [InlineData("I have to go now")]
    [InlineData("i need to go feed the animals")]
    [InlineData("I should get going")]
    [InlineData("see you later")]
    [InlineData("see ya")]
    [InlineData("cya")]
    [InlineData("talk to you later")]
    [InlineData("ttyl")]
    [InlineData("good night")]
    [InlineData("Goodnight Linus")]
    [InlineData("later")]
    [InlineData("I'm off")]
    [InlineData("im leaving")]
    [InlineData("I'm heading home")]
    [InlineData("take care")]
    [InlineData("farewell")]
    public void ExplicitFarewellsAreDetected(string message) =>
        Assert.True(ConversationSignals.IsPlayerFarewell(message));

    [Theory]
    [InlineData("no")]                                  // refusal, not necessarily leaving (trained marker's job)
    [InlineData("Nah")]
    [InlineData("Ew")]
    [InlineData("bruhhh")]
    [InlineData("Whatever")]
    [InlineData("what?????")]
    [InlineData("Is she really your friend?")]
    [InlineData("do you need to go anywhere?")]         // second person: asking Linus, not leaving
    [InlineData("you should go to the festival")]
    [InlineData("Did you ever go to town?")]
    [InlineData("maybe")]                                // contains "bye" letters but not the word
    [InlineData("what happened later that night")]      // "later" mid-sentence is not a sign-off
    [InlineData("Tell me about the winters up here.")]
    public void OrdinaryMessagesAreNotFarewells(string message) =>
        Assert.False(ConversationSignals.IsPlayerFarewell(message));

    // ---- trained end-of-conversation marker -------------------------------------------------

    [Fact]
    public void EndMarkerIsStrippedAndSignalled()
    {
        bool ended = ConversationSignals.TryStripEndMarker(
            "Go gently, my friend. The mountain will be here when you return. [end]", out string cleaned);
        Assert.True(ended);
        Assert.Equal("Go gently, my friend. The mountain will be here when you return.", cleaned);
    }

    [Fact]
    public void MarkerIsCaseInsensitive()
    {
        Assert.True(ConversationSignals.TryStripEndMarker("Safe travels. [End]", out string cleaned));
        Assert.Equal("Safe travels.", cleaned);
    }

    [Fact]
    public void RepliesWithoutMarkerPassThrough()
    {
        Assert.False(ConversationSignals.TryStripEndMarker("The fire is warm tonight.", out string cleaned));
        Assert.Equal("The fire is warm tonight.", cleaned);
    }

    [Fact]
    public void StrayMidReplyMarkerStillEndsAndIsRemoved()
    {
        Assert.True(ConversationSignals.TryStripEndMarker("Rest well. [end] The stars are out.", out string cleaned));
        Assert.DoesNotContain("[end]", cleaned, StringComparison.OrdinalIgnoreCase);
    }
}
