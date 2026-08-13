using ChattyValley.Core;

namespace ChattyValley.Tests;

/// <summary>
/// The location-leak defect found tracing the Elliott stage-3 path (2026-08-13): the old map covered
/// four names and fell through to the RAW Stardew map name for everything else. Linus never left the
/// mountains so it never showed, but Elliott's schedule lives in ElliottHouse, ArchaeologyHouse and
/// the Saloon, which would have reached the prompt verbatim.
///
/// This matters because the adapter's system line is minimal precisely so that training format
/// equals inference format. A raw map name is input the adapter never saw in training.
/// </summary>
public class LocationsTests
{
    [Theory]
    [InlineData("Mountain", "the mountains")]
    [InlineData("Forest", "the forest")]
    [InlineData("Town", "Pelican Town")]
    [InlineData("Beach", "the beach")]
    public void KeepsTheOriginalFourMappings(string map, string expected) =>
        Assert.Equal(expected, Locations.Friendly(map));

    [Theory]
    [InlineData("ElliottHouse", "Elliott's cabin")]
    [InlineData("ArchaeologyHouse", "the library")]
    [InlineData("Saloon", "the Stardrop Saloon")]
    [InlineData("SeedShop", "Pierre's shop")]
    [InlineData("FishShop", "Willy's shop")]
    [InlineData("Hospital", "the clinic")]
    [InlineData("IslandSouth", "the island resort")]
    public void MapsTheLocationsElliottsScheduleReaches(string map, string expected) =>
        Assert.Equal(expected, Locations.Friendly(map));

    [Theory]
    [InlineData("ScienceHouse")]
    [InlineData("UndergroundMine")]
    [InlineData("BathHouse_Entry")]
    [InlineData("Trailer_Big")]
    public void UnmappedNamesNeverReachThePrompt(string map)
    {
        var friendly = Locations.Friendly(map);
        Assert.Equal(Locations.Fallback, friendly);
        Assert.NotEqual(map, friendly);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingNameFallsBackInWorld(string? map) =>
        Assert.Equal(Locations.Fallback, Locations.Friendly(map));

    [Fact]
    public void FallbackIsAnInWorldPhrase() => Assert.Equal("the valley", Locations.Fallback);

    [Fact]
    public void LookupIsCaseInsensitive() =>
        Assert.Equal("the library", Locations.Friendly("archaeologyhouse"));

    [Fact]
    public void NoMappedPhraseLooksLikeARawMapName()
    {
        // A raw Stardew map name is PascalCase with no spaces. Every phrase we inject should read as
        // English instead, which is the property that broke.
        // "JojaMart" is exempt: the compound IS the in-world brand name, so it is what the town
        // would say. Every other entry has to read as English rather than as an engine identifier.
        var brandNames = new HashSet<string> { "JojaMart" };

        foreach (var map in new[]
                 {
                     "Mountain", "Forest", "Town", "Beach", "BusStop", "Railroad", "Woods", "Desert",
                     "Farm", "IslandSouth", "IslandWest", "Saloon", "ArchaeologyHouse", "SeedShop",
                     "FishShop", "Hospital", "Blacksmith", "AnimalShop", "JojaMart", "CommunityCenter",
                     "ElliottHouse", "LeahHouse",
                 })
        {
            var phrase = Locations.Friendly(map);
            Assert.True(Locations.IsMapped(map), $"{map} should be mapped");
            if (brandNames.Contains(map))
                continue;
            Assert.True(phrase.Contains(' '), $"{map} -> '{phrase}' reads like a raw map name");
        }
    }
}
