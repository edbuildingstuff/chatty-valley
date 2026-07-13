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
        var sb = new StringBuilder();
        sb.Append($"You are {c.Name}, a resident of Pelican Town in Stardew Valley. ");
        sb.Append(c.Bio.Trim());
        sb.Append($" Reply as {c.Name} speaking in the first person, with only their spoken words. ");
        sb.Append("Do not narrate actions, do not write stage directions, and do not write the other person's lines. ");
        sb.Append("Keep replies to 1 to 3 short sentences. Never break character, and never mention being an AI or being in a game.\n\n");
        sb.Append("Current situation: ").Append(ctx.ToContextLine());

        // Stage 1a only: anchor the voice with a few canon lines. Once an adapter carries the voice
        // (AdapterPath set), the anchors drop away and the prompt shrinks. That shrinkage is the story.
        if (c.AdapterPath is null && c.FewShot.Count > 0)
        {
            sb.Append("\n\nFor voice reference, some lines this character has said before:\n");
            foreach (var line in c.FewShot)
                sb.Append("- \"").Append(line).Append("\"\n");
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
