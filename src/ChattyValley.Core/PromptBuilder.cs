using System.Text;

namespace ChattyValley.Core;

/// <summary>
/// Builds the villager prompt stable-context-first (plan doc 04 section 5): identity, bio, and
/// world context form the stable, cacheable prefix; the volatile player turn goes last, so a KV
/// prefix cache only recomputes the tail each turn.
/// </summary>
public sealed class PromptBuilder
{
    private readonly ChatTemplate _template;

    public PromptBuilder(ChatTemplate template) => _template = template;

    public string BuildSystem(Character c, GameContext ctx)
    {
        // Stage 1b (adapter): the voice lives in the weights, so the system message shrinks to the
        // identity tag + a compact game-state line. This MUST match the training-data system format
        // (dataset-plan.md: "system content stays minimal ... training format equals inference
        // format") or the adapter sees out-of-distribution input and its measured quality drops.
        if (c.AdapterPath is not null)
            return $"You are {c.Name}, a resident of Pelican Town in Stardew Valley. "
                 + $"Current situation: {AdapterContext(ctx)}";

        // Stage 1a (stock base): the full instruction block plus few-shot voice anchors carry the
        // character, because the base has no trained voice of its own.
        var sb = new StringBuilder();
        sb.Append($"You are {c.Name}, a resident of Pelican Town in Stardew Valley. ");
        sb.Append(c.Bio.Trim());
        sb.Append($" Reply as {c.Name} speaking in the first person, with only their spoken words. ");
        sb.Append("Do not narrate actions, do not write stage directions, and do not write the other person's lines. ");
        sb.Append("Keep replies to 1 to 3 short sentences. Never break character, and never mention being an AI or being in a game.\n\n");
        sb.Append("Current situation: ").Append(ctx.ToContextLine());

        if (c.FewShot.Count > 0)
        {
            sb.Append("\n\nFor voice reference, some lines this character has said before:\n");
            foreach (var line in c.FewShot)
                sb.Append("- \"").Append(line).Append("\"\n");
        }
        return sb.ToString();
    }

    // Compact context line for adapter mode, matching the training-data format, e.g.
    // "winter, snowing, evening, the mountains, 6 hearts" (+ a festival or gift clause when present).
    private static string AdapterContext(GameContext ctx)
    {
        var weather = ctx.Weather switch { "snow" => "snowing", "rain" => "raining", _ => ctx.Weather };
        var sb = new StringBuilder();
        sb.Append($"{ctx.Season}, {weather} {ctx.TimeOfDay}, {ctx.Location}, {ctx.Hearts} hearts");
        if (!string.IsNullOrEmpty(ctx.Event))
            sb.Append($", {ctx.Event}");
        if (!string.IsNullOrEmpty(ctx.Gift))
        {
            var taste = ctx.GiftTaste switch
            {
                "love" => " (he loves it)", "like" => " (he likes it)",
                "dislike" => " (he dislikes it)", "hate" => " (he hates it)",
                "neutral" => " (he is indifferent to it)", _ => "",
            };
            sb.Append($", {ctx.PlayerName} offering a {ctx.Gift}{taste}");
        }
        return sb.ToString();
    }

    // The user turn is the player's own spoken words, with no stage directions or narration cues:
    // small models will otherwise echo a "[Sam walks up...]" direction and continue it as prose.
    public string BuildUserTurn(string? playerMessage, GameContext ctx) =>
        playerMessage ?? "Hello!";

    public string Build(Character c, GameContext ctx, string? playerMessage) =>
        _template.Render(BuildSystem(c, ctx), BuildUserTurn(playerMessage, ctx));
}
