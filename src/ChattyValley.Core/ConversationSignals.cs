using System.Text.RegularExpressions;

namespace ChattyValley.Core;

/// <summary>
/// End-of-conversation signals, shared by the mod and the harness. Hybrid contract:
///
/// 1. <see cref="IsPlayerFarewell"/> is a conservative runtime heuristic for EXPLICIT goodbyes
///    ("bye", "gotta go", "see you later"). A false positive closes a chat the player wanted to
///    continue, so ambiguous dismissals ("no", "whatever", "bruhhh") deliberately do NOT match.
/// 2. The trained <see cref="EndMarker"/> covers those messy cases: v3+ adapters are trained to end
///    farewell replies with the marker (data/linus/batches/farewell.md), and the runtime closes the
///    conversation when a reply carries it. The marker is stripped before display.
/// </summary>
public static class ConversationSignals
{
    /// <summary>Trained end-of-conversation marker; farewell replies in v3+ training data end with it.</summary>
    public const string EndMarker = "[end]";

    // Whole-message farewells after normalisation (lowercase, apostrophes and punctuation removed).
    // These words are goodbyes only when they are the entire message ("later" mid-sentence is not).
    private static readonly HashSet<string> WholeMessage = new(StringComparer.Ordinal)
    {
        "bye", "byebye", "bye bye", "goodbye", "good bye", "farewell",
        "later", "laters", "cya", "see ya", "seeya", "gtg", "g2g", "ttyl",
        "good night", "goodnight", "night", "take care", "peace", "peace out",
        "im out", "im off", "im done", "im leaving", "gotta go", "gotta run",
    };

    // Phrase farewells that are unambiguous wherever they appear in the message. First-person
    // patterns require the leading "i" so questions about the OTHER person ("do you need to go?")
    // never match.
    private static readonly Regex Phrase = new(
        @"\b(good ?bye|bye now|bye bye|farewell|"
        + @"gotta (go|run|head)|got to go|"
        + @"i (have|need|got|must|ought) to (go|leave|run|head)|"
        + @"i (should|d better|better) (go|get going|head)|i must be going|"
        + @"im (leaving|off|out of here|heading (home|out|back))|"
        + @"see (you|ya) (later|soon|tomorrow|around)|catch you later|talk to you later|ttyl|"
        + @"good ?night)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Does the player's typed message read as an explicit goodbye?</summary>
    public static bool IsPlayerFarewell(string message)
    {
        string norm = Normalize(message);
        if (norm.Length == 0) return false;
        if (WholeMessage.Contains(norm)) return true;
        // "bye <name>" ("bye Linus") but not "bye" buried mid-sentence, where it is usually
        // quoted or narrated rather than said to the villager.
        if (norm.StartsWith("bye ", StringComparison.Ordinal)) return true;
        return Phrase.IsMatch(norm);
    }

    /// <summary>
    /// Default false-premise guard clause. Appended to the system turn ONLY when
    /// <see cref="LooksLikeFalsePremise"/> flags the player's message, to reduce the model playing
    /// along with a fabricated event smuggled in as a question's premise. Prompt-level testing showed
    /// it roughly halves how often the model adopts a false premise. Tunable via config.
    /// </summary>
    public const string DefaultFalsePremiseGuard =
        "The player may mention things that never happened. If you do not remember it, say so plainly.";

    // Player turns that PRESUPPOSE a third-party event, gift, or shared past ("when Leah visited your
    // tent yesterday, what did you talk about?"). The failure this guards is the model answering the
    // surface question and swallowing the fabricated premise. Precision matters more than recall: a
    // false fire nags during ordinary talk, so relationship queries ("do you know X", "are you close
    // with X") and normal chat deliberately do NOT match; only event/gift/shared-past presuppositions do.
    private static readonly Regex FalsePremise = new(
        @"\b(when \w+ (visited|came|stopped by|dropped by|brought|gave|told you|showed you|helped you|"
        + @"read you|played you|sang to you|cooked you|made you|wrote you|was (here|up here|over))|"
        + @"remember when (we|you)|remember (that time|how you|teaching me|when i)|back when (you|we)|"
        + @"since you (and \w+ )?(used to|dated|were)|you used to (date|know|see) \w+|"
        + @"(after|when) you (and|&) \w+ (argued|fought|fell out|split|made up)|"
        + @"what did you and \w+ (talk about|do|get up to|discuss|jam|argue|fight|chat|sing|play)|"
        + @"the \w+ (you|that you) (built|buried|made|helped|planted|gave)|"
        + @"everyone (knows|says) you (were|built|used to)|\w+ (said|told me|says) you (two|and)|"
        + @"how did you (like|enjoy) the \w+( \w+)? (brought|gave|made|knitted|baked))\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Does the player's message presuppose a fabricated third-party event/gift/shared-past? When
    /// true the caller appends the false-premise guard clause to the system turn for this turn only.
    /// </summary>
    public static bool LooksLikeFalsePremise(string message) =>
        !string.IsNullOrWhiteSpace(message) && FalsePremise.IsMatch(message);

    /// <summary>
    /// If <paramref name="reply"/> carries the trained end marker, remove every occurrence (it is
    /// never player-facing text) and return true: the conversation should close after this line.
    /// </summary>
    public static bool TryStripEndMarker(string reply, out string cleaned)
    {
        cleaned = Regex.Replace(reply, @"\s*\[end\]", "", RegexOptions.IgnoreCase).Trim();
        return cleaned.Length != reply.Trim().Length;
    }

    private static string Normalize(string message)
    {
        string lower = message.ToLowerInvariant().Replace("'", "").Replace("’", "");
        lower = Regex.Replace(lower, @"[^a-z0-9 ]", " ");
        return Regex.Replace(lower, @"\s+", " ").Trim();
    }
}
