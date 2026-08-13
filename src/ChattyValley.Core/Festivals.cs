namespace ChattyValley.Core;

/// <summary>
/// Stardew's festival calendar, as the phrase a villager would use.
///
/// Festival dates are fixed content, so this is a static (season, day) lookup rather than a call
/// into the game. That keeps it unit testable, keeps the mod project free of another API surface to
/// get wrong, and means the phrase the model sees is authored here rather than assembled from a
/// display string at runtime.
///
/// The phrase includes its own article, because the article is not uniform: "the Flower Dance" but
/// "Spirit's Eve". Callers append the value as-is.
/// </summary>
public static class Festivals
{
    // Keyed by season, then day of month. Names follow Data/Festivals, which is what the town
    // actually calls them; the article is chosen per name.
    private static readonly Dictionary<string, Dictionary<int, string>> Calendar =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["spring"] = new()
            {
                [13] = "the Egg Festival",
                // The Desert Festival (1.6) is a passive festival running Spring 15 to 17.
                [15] = "the Desert Festival",
                [16] = "the Desert Festival",
                [17] = "the Desert Festival",
                [24] = "the Flower Dance",
            },
            ["summer"] = new()
            {
                [11] = "the Luau",
                [28] = "the Dance of the Moonlight Jellies",
            },
            ["fall"] = new()
            {
                [16] = "the Stardew Valley Fair",
                [27] = "Spirit's Eve",
            },
            ["winter"] = new()
            {
                [8] = "the Festival of Ice",
                // The Night Market is a passive festival running Winter 15 to 17.
                [15] = "the Night Market",
                [16] = "the Night Market",
                [17] = "the Night Market",
                [25] = "the Feast of the Winter Star",
            },
        };

    /// <summary>
    /// The festival phrase for a calendar day, or null on an ordinary day. Season is matched
    /// case-insensitively because <c>Game1.currentSeason</c> is a lowercase string.
    /// </summary>
    public static string? ForDay(string? season, int day) =>
        season is not null && Calendar.TryGetValue(season, out var days) && days.TryGetValue(day, out var name)
            ? name
            : null;

    /// <summary>Every distinct festival phrase, for tests and for authoring reference.</summary>
    public static IReadOnlyCollection<string> All =>
        Calendar.Values.SelectMany(d => d.Values).Distinct().ToList();
}
