using System.Text;

namespace ChattyValley.Core;

/// <summary>
/// A live game-state snapshot fed into a villager prompt. Mirrors the context schema in the plan
/// (research/stardew-valley-mod/04-tier1-prototype-architecture.md section 5). The harness fakes
/// these values; the SMAPI mod will read them from the running game.
/// </summary>
public sealed class GameContext
{
    public string Season { get; init; } = "spring";
    public int Day { get; init; } = 1;
    public int Year { get; init; } = 1;
    public string Weather { get; init; } = "clear";
    public string TimeOfDay { get; init; } = "morning"; // morning / afternoon / evening
    public string Clock { get; init; } = "9:00 AM";
    public string Weekday { get; init; } = "Monday";
    public string Location { get; init; } = "the mountains";
    public int Hearts { get; init; }
    public int FriendshipPoints { get; init; }              // out of 250 toward the next heart
    public string Relationship { get; init; } = "acquaintance"; // acquaintance / friend / dating / engaged / married
    public string? Event { get; init; }                     // festival or event today
    public string? Gift { get; init; }                      // item currently held out as a gift
    public string? GiftTaste { get; init; }                 // love / like / neutral / dislike / hate
    public string PlayerName { get; init; } = "Sam";
    public string FarmName { get; init; } = "Willow";

    /// <summary>Human-readable context block. Stable-first ordering is handled by PromptBuilder.</summary>
    public string ToContextLine()
    {
        var sb = new StringBuilder();
        sb.Append($"It is {Weather} on {Weekday}, day {Day} of {Season} (year {Year}), {TimeOfDay} ({Clock}). ");
        sb.Append($"You are at {Location}. ");
        sb.Append($"Your friendship with {PlayerName} is {Hearts} hearts ({FriendshipPoints}/250 toward the next), {Relationship}.");
        if (!string.IsNullOrEmpty(Event))
            sb.Append($" Today is {Event}.");
        if (!string.IsNullOrEmpty(Gift))
            sb.Append(GiftTaste is null
                ? $" {PlayerName} is holding out a {Gift}."
                : $" {PlayerName} is holding out a {Gift}, which you {GiftTaste}.");
        return sb.ToString();
    }
}
