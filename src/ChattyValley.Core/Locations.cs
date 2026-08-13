namespace ChattyValley.Core;

/// <summary>
/// Stardew's internal map names translated into the words the villagers actually use, for the
/// game-state line in the adapter prompt.
///
/// Why this exists (found tracing the Elliott stage-3 path, 2026-08-13): the mod used to map four
/// names and fall through to the RAW map name for everything else. Linus never left the mountains,
/// so it never showed. Elliott's own event-file keys are <c>ElliottHouse</c>, <c>ArchaeologyHouse</c>
/// and <c>Saloon</c>, so the runtime would have injected "Current situation: fall, clear afternoon,
/// ArchaeologyHouse, 6 hearts" while the training data says "the library".
///
/// The adapter's system line is deliberately minimal because training format equals inference
/// format (see <see cref="PromptBuilder.BuildSystem"/>), so an unmapped raw name is out-of-
/// distribution input, not a cosmetic blemish. The fallback therefore returns a safe in-voice
/// phrase rather than the raw name: a vague-but-in-world location costs nothing, and a leaked
/// <c>ScienceHouse</c> costs a reply.
/// </summary>
public static class Locations
{
    /// <summary>What an unmapped or unknown map is called. Always in-world, never a raw map name.</summary>
    public const string Fallback = "the valley";

    // Names marked (canon) appear verbatim as event-file keys in the villagers' extracted dialogue
    // under data/*/raw/, so they are confirmed against game content rather than assumed.
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        // outdoors
        ["Mountain"] = "the mountains",           // (canon)
        ["Forest"] = "the forest",
        ["Town"] = "Pelican Town",                // (canon)
        ["Beach"] = "the beach",                  // (canon)
        ["BusStop"] = "the bus stop",
        ["Railroad"] = "the railroad",
        ["Woods"] = "the woods",
        ["Desert"] = "the desert",
        ["Farm"] = "the farm",
        ["IslandSouth"] = "the island resort",    // (canon)
        ["IslandWest"] = "the island",

        // the town's public rooms, which is where most of the roster's schedules land
        ["Saloon"] = "the Stardrop Saloon",       // (canon)
        ["ArchaeologyHouse"] = "the library",     // (canon)
        ["SeedShop"] = "Pierre's shop",
        ["FishShop"] = "Willy's shop",
        ["Hospital"] = "the clinic",
        ["Blacksmith"] = "the blacksmith's",
        ["AnimalShop"] = "Marnie's ranch",
        ["JojaMart"] = "JojaMart",
        ["CommunityCenter"] = "the community center",

        // villager homes on the roster's paths
        ["ElliottHouse"] = "Elliott's cabin",     // (canon)
        ["LeahHouse"] = "Leah's cottage",
    };

    /// <summary>
    /// The in-voice phrase for a Stardew map name. Null, blank, or anything unmapped becomes
    /// <see cref="Fallback"/>, so the prompt never carries a raw internal name.
    /// </summary>
    public static string Friendly(string? mapName)
    {
        if (string.IsNullOrWhiteSpace(mapName))
            return Fallback;
        return Map.TryGetValue(mapName, out var friendly) ? friendly : Fallback;
    }

    /// <summary>True when <paramref name="mapName"/> has an explicit in-voice phrase.</summary>
    public static bool IsMapped(string? mapName) =>
        !string.IsNullOrWhiteSpace(mapName) && Map.ContainsKey(mapName);
}
