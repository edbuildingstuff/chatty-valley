namespace ChattyValley.Core;

/// <summary>
/// The sliding history window sent to the model each turn, shared by the mod and the harness so
/// probe runs replay exactly what the mod sends in-game.
///
/// Invariant (the 2026-07-23 play-test bug): history always ends on the player's latest message,
/// so at ask time it has odd length, and slicing an even <c>max</c> off the end opened every
/// post-slide prompt with an orphaned villager reply (system -> assistant -> user...). The adapter
/// only ever trained on user-first conversations, so replies detached from the question that
/// prompted them. The window therefore always opens on a user turn: after slicing, any leading
/// assistant turn is dropped.
/// </summary>
public static class ConversationWindow
{
    /// <summary>
    /// Return the most recent messages of <paramref name="history"/>, at most <paramref name="max"/>
    /// (minimum 2), opening on a user turn and ending on the latest message.
    /// </summary>
    public static IReadOnlyList<ChatTurn> Apply(IReadOnlyList<ChatTurn> history, int max)
    {
        max = Math.Max(2, max);
        if (history.Count <= max && (history.Count == 0 || history[0].IsUser))
            return history;

        var slice = new List<ChatTurn>(max);
        for (int i = Math.Max(0, history.Count - max); i < history.Count; i++)
            slice.Add(history[i]);
        while (slice.Count > 0 && !slice[0].IsUser)
            slice.RemoveAt(0);
        return slice;
    }
}
