using ChattyValley.Core;

namespace ChattyValley.Tests;

/// <summary>
/// The gift and festival clauses, wired 2026-08-13 (DAT-745).
///
/// Until then <c>ModEntry.ReadContext</c> populated thirteen fields and set none of Event, Gift or
/// GiftTaste, so 54 rows of Elliott's training data described a context the shipped mod could never
/// emit. These tests pin the rendered shape, because the adapter's system line IS the prompt:
/// training format has to equal inference format or the model sees input it never trained on.
/// </summary>
public class ContextClauseTests
{
    private static GameContext Ctx(string? gift = null, string? taste = null, string? evt = null) => new()
    {
        Season = "fall", Weather = "clear", TimeOfDay = "afternoon",
        Location = "the beach", Hearts = 6, Event = evt, Gift = gift, GiftTaste = taste,
    };

    private static string Adapter(GameContext ctx)
    {
        var c = new Character { Name = "Elliott", AdapterPath = "x.gguf" };
        return new PromptBuilder(ChatTemplate.Lfm2).BuildSystem(c, ctx);
    }

    // ---- gift clause --------------------------------------------------------------------------

    [Fact]
    public void GiftClauseCarriesNoArticle()
    {
        // "a Crab Cakes" and "a Milk" were the reason the article went. Plurals and mass nouns are
        // ordinary in the game's display names, so no single article is correct.
        Assert.EndsWith(", @ offering Crab Cakes (he loves it)", Adapter(Ctx("Crab Cakes", "love")));
        Assert.EndsWith(", @ offering Milk (he dislikes it)", Adapter(Ctx("Milk", "dislike")));
        Assert.EndsWith(", @ offering Octopus (he likes it)", Adapter(Ctx("Octopus", "like")));
    }

    [Fact]
    public void GiftClauseRendersEveryTaste()
    {
        Assert.Contains("(he loves it)", Adapter(Ctx("Squid Ink", "love")));
        Assert.Contains("(he likes it)", Adapter(Ctx("Banana", "like")));
        Assert.Contains("(he dislikes it)", Adapter(Ctx("Pizza", "dislike")));
        Assert.Contains("(he hates it)", Adapter(Ctx("Quartz", "hate")));
        Assert.Contains("(he is indifferent to it)", Adapter(Ctx("Coral", "neutral")));
    }

    [Fact]
    public void UnknownTasteDropsTheParentheticalButKeepsTheItem()
    {
        var s = Adapter(Ctx("Prismatic Shard", taste: null));
        Assert.EndsWith(", @ offering Prismatic Shard", s);
    }

    [Fact]
    public void NoGiftMeansNoClause()
    {
        Assert.DoesNotContain("offering", Adapter(Ctx()));
    }

    // ---- festival clause ----------------------------------------------------------------------

    [Fact]
    public void FestivalClauseIsAppendedVerbatim()
    {
        Assert.EndsWith(", the Flower Dance", Adapter(Ctx(evt: "the Flower Dance")));
    }

    [Fact]
    public void FestivalAndGiftBothRenderInOrder()
    {
        var s = Adapter(Ctx("Duck Feather", "love", "the Feast of the Winter Star"));
        Assert.EndsWith(", the Feast of the Winter Star, @ offering Duck Feather (he loves it)", s);
    }

    // ---- the calendar -------------------------------------------------------------------------

    [Theory]
    [InlineData("spring", 13, "the Egg Festival")]
    [InlineData("spring", 24, "the Flower Dance")]
    [InlineData("summer", 11, "the Luau")]
    [InlineData("summer", 28, "the Dance of the Moonlight Jellies")]
    [InlineData("fall", 16, "the Stardew Valley Fair")]
    [InlineData("fall", 27, "Spirit's Eve")]
    [InlineData("winter", 8, "the Festival of Ice")]
    [InlineData("winter", 25, "the Feast of the Winter Star")]
    public void CalendarKnowsEveryMainFestival(string season, int day, string expected) =>
        Assert.Equal(expected, Festivals.ForDay(season, day));

    [Theory]
    [InlineData("spring", 15)]
    [InlineData("spring", 16)]
    [InlineData("spring", 17)]
    public void DesertFestivalRunsThreeDays(string season, int day) =>
        Assert.Equal("the Desert Festival", Festivals.ForDay(season, day));

    [Theory]
    [InlineData("winter", 15)]
    [InlineData("winter", 16)]
    [InlineData("winter", 17)]
    public void NightMarketRunsThreeDays(string season, int day) =>
        Assert.Equal("the Night Market", Festivals.ForDay(season, day));

    [Theory]
    [InlineData("spring", 1)]
    [InlineData("summer", 14)]
    [InlineData("fall", 3)]
    [InlineData("winter", 28)]
    public void OrdinaryDaysCarryNoFestival(string season, int day) =>
        Assert.Null(Festivals.ForDay(season, day));

    [Fact]
    public void SeasonLookupIsCaseInsensitive()
    {
        // Game1.currentSeason is lowercase; authoring and tests are not always.
        Assert.Equal("the Luau", Festivals.ForDay("Summer", 11));
        Assert.Null(Festivals.ForDay(null, 11));
    }

    [Fact]
    public void SpiritsEveCarriesNoArticleWhileTheOthersDo()
    {
        // The article is part of the phrase because it is not uniform. This is why the renderer
        // appends the value as-is rather than prefixing "the".
        Assert.Equal("Spirit's Eve", Festivals.ForDay("fall", 27));
        Assert.All(Festivals.All.Where(f => f != "Spirit's Eve"), f => Assert.StartsWith("the ", f));
    }

    // ---- taste codes --------------------------------------------------------------------------

    [Theory]
    [InlineData(GiftTastes.Love, "love")]
    [InlineData(GiftTastes.Like, "like")]
    [InlineData(GiftTastes.Dislike, "dislike")]
    [InlineData(GiftTastes.Hate, "hate")]
    [InlineData(GiftTastes.Neutral, "neutral")]
    public void GameTasteCodesMapToThePromptWords(int code, string expected) =>
        Assert.Equal(expected, GiftTastes.Describe(code));

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(-1)]
    [InlineData(99)]
    public void UnrecognisedTasteCodesYieldNull(int code) => Assert.Null(GiftTastes.Describe(code));
}
