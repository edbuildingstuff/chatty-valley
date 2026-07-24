using ChattyValley.Core;

namespace ChattyValley.Tests;

/// <summary>
/// Training format equals inference format (dataset-plan.md). The training data uses Stardew's
/// own "@" as the player-name placeholder (61/751 conversations), so the rendered system prompt
/// must too; the real name is substituted at display time by the mod, never in the prompt.
/// The 2026-07-23 trace showed the gift clause leaking the real name into the prompt while the
/// model correctly emitted "@" it had learned from training.
/// </summary>
public class PromptBuilderTests
{
    private static readonly Character Adapter = new()
    {
        Name = "Linus", Bio = "unused in adapter mode", AdapterPath = "linus.gguf",
    };

    private static GameContext GiftContext => new()
    {
        Season = "fall", Weather = "clear", TimeOfDay = "afternoon", Location = "the mountains",
        Hearts = 4, PlayerName = "Edward", Gift = "blackberry", GiftTaste = "love",
    };

    [Fact]
    public void GiftClauseUsesPlaceholderNotRealName()
    {
        var prompt = new PromptBuilder(ChatTemplate.Lfm2);
        string system = prompt.BuildSystem(Adapter, GiftContext);
        Assert.Contains("@ offering a blackberry", system);
        Assert.DoesNotContain("Edward", system);
    }

    [Fact]
    public void AdapterSystemPromptMatchesTrainingShape()
    {
        var prompt = new PromptBuilder(ChatTemplate.Lfm2);
        string system = prompt.BuildSystem(Adapter, GiftContext);
        Assert.StartsWith("You are Linus, a resident of Pelican Town in Stardew Valley. Current situation: ", system);
        Assert.Contains("fall, clear afternoon, the mountains, 4 hearts", system);
    }

    // ---- false-premise guard injection (conditional; see ConversationSignals.LooksLikeFalsePremise) --

    private static GameContext PlainContext => new()
    {
        Season = "spring", Weather = "clear", TimeOfDay = "morning", Location = "the mountains", Hearts = 4,
    };

    private const string Guard = ConversationSignals.DefaultFalsePremiseGuard;

    private static List<ChatTurn> History(string playerTurn) => new() { new ChatTurn(true, playerTurn) };

    [Fact]
    public void GuardInjectedWhenLatestTurnIsFalsePremise()
    {
        var prompt = new PromptBuilder(ChatTemplate.Lfm2);
        string p = prompt.BuildConversation(Adapter, PlainContext,
            History("So when Leah came up to your tent yesterday, what did you talk about?"), Guard);
        Assert.Contains("say so plainly", p);
    }

    [Fact]
    public void GuardNotInjectedOnOrdinaryTurn()
    {
        var prompt = new PromptBuilder(ChatTemplate.Lfm2);
        string p = prompt.BuildConversation(Adapter, PlainContext,
            History("Do you know Leah, the artist?"), Guard);
        Assert.DoesNotContain("say so plainly", p);
    }

    [Fact]
    public void GuardNotInjectedWhenDisabled()
    {
        var prompt = new PromptBuilder(ChatTemplate.Lfm2);
        string p = prompt.BuildConversation(Adapter, PlainContext,
            History("So when Leah came up to your tent yesterday, what did you talk about?"), falsePremiseGuard: null);
        Assert.DoesNotContain("say so plainly", p);
    }

    [Fact]
    public void GuardChecksLatestTurnNotEarlierOnes()
    {
        // A false premise earlier in the history must not keep re-firing the guard on later, ordinary turns.
        var prompt = new PromptBuilder(ChatTemplate.Lfm2);
        var history = new List<ChatTurn>
        {
            new(true, "So when Leah came up to your tent yesterday, what did you talk about?"),
            new(false, "Leah has not been up here, friend."),
            new(true, "Fair enough. What is the weather like up here in winter?"),
        };
        string p = prompt.BuildConversation(Adapter, PlainContext, history, Guard);
        Assert.DoesNotContain("say so plainly", p);
    }
}
