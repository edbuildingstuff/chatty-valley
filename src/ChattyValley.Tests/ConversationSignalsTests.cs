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

    // ---- false-premise detector -------------------------------------------------------------
    // Fires only on turns that PRESUPPOSE a third-party event/gift/shared past (the guard's target).
    // Precision matters more than recall: a false fire nags ordinary talk, so relationship queries
    // and normal chat must NOT match.

    [Theory]
    [InlineData("So when Leah came up to your tent yesterday, what did you two talk about?")]
    [InlineData("when Sebastian stopped by your fire last night, did he seem alright?")]
    [InlineData("Since you and Caroline used to be sweet on each other, do you still talk?")]
    [InlineData("How did you like the scarf Emily knitted you for the Winter Star?")]
    [InlineData("Remember when you taught me to forage that first spring I got here?")]
    [InlineData("So when Elliott read you his new chapter up here, what did you make of it?")]
    [InlineData("Everyone knows you built the old community center. What was that like?")]
    [InlineData("What did you and Sam jam on when he brought his guitar up the mountain?")]
    [InlineData("What did you and Shane argue about outside the ranch?")]
    [InlineData("So the treasure you buried by the lake, is it still there?")]
    [InlineData("Abigail told me you two are close.")]
    // DAT-754: habitual-companion presuppositions (the DAT-743 triage measured 8/8 adoption on
    // these with zero detector fires; definite reference is what marks them as presupposed)
    [InlineData("Tell me about the heron that visits you.")]
    [InlineData("How's the fox you feed doing?")]
    [InlineData("Tell me about the owl that sleeps above your tent.")]
    [InlineData("Is the stray dog still coming by your camp?")]
    [InlineData("What do you feed the crows that follow you everywhere?")]
    [InlineData("Does your tame rabbit like the winter?")]
    [InlineData("I saw the deer you befriended down by the lake.")]
    [InlineData("How are your pets?")]
    public void FalsePremisesAreDetected(string message) =>
        Assert.True(ConversationSignals.LooksLikeFalsePremise(message));

    [Theory]
    [InlineData("You know your neighbour Robin pretty well, I would guess?")]  // relationship query, TRUE
    [InlineData("Are you and the Wizard on good terms?")]
    [InlineData("Do you know George and Evelyn in town?")]
    [InlineData("You must know Gus down at the saloon.")]
    [InlineData("Do you know Leah, the artist?")]
    [InlineData("Hello, Linus.")]
    [InlineData("Are you AI?")]
    [InlineData("What do you think about data centres?")]
    [InlineData("Have you been to the town at all?")]
    [InlineData("Do you get lonely up here?")]
    [InlineData("What is your favourite food?")]
    [InlineData("Did you ever go to town?")]
    [InlineData("When do you forage in the spring?")]
    [InlineData("bye Linus")]
    [InlineData("")]
    // DAT-754 precision boundary: open questions about animals carry no presupposed companion,
    // and the bare-infinitive continuation is a deliberate recall trade (see the findings doc)
    [InlineData("Do you have any animal friends up here?")]
    [InlineData("Is there a bird or animal that keeps you company by the tent?")]
    [InlineData("Do you keep pets?")]
    [InlineData("Does the Wizard still visit you?")]
    public void OrdinaryTurnsAreNotFalsePremises(string message) =>
        Assert.False(ConversationSignals.LooksLikeFalsePremise(message));
}
