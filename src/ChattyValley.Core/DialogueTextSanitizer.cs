using System.Linq;
using System.Text.RegularExpressions;

namespace ChattyValley.Core;

/// <summary>
/// Prepares a model reply for Stardew's vanilla <c>Dialogue</c> parser, which treats several
/// characters as control codes: "#" splits dialogue boxes, "^" switches on farmer gender, "$"
/// prefixes emotion/command codes ("$h", "$q"...), "{"/"}" wrap tokens, and "@" substitutes the
/// player name. Model output is plain prose so these are rare, but a single stray "#" would
/// truncate the box mid-sentence. "@" is OUR placeholder too, so it is substituted here
/// deterministically rather than left to the game.
/// </summary>
public static class DialogueTextSanitizer
{
    /// <summary>Make <paramref name="reply"/> safe to hand to <c>new Dialogue(...)</c> verbatim.</summary>
    public static string Sanitize(string reply, string playerName)
    {
        string s = reply.Replace("@", playerName);
        s = s.Replace("#", ",");            // box-split -> plain pause
        s = s.Replace("^", " ");            // gender switch -> space
        s = s.Replace("{", "").Replace("}", "");
        s = Regex.Replace(s, @"\$(?=\w)", "");   // "$h"-style codes -> bare text; lone "$" is safe
        // Base-model bleed (DAT-745 stage 5): sampled replies occasionally end with a stray "**" or
        // carry one unmatched double quote. Neither has a meaning in a villager's line, so asterisks go
        // entirely and a quote is kept only when it has a partner.
        s = s.Replace("*", "");
        if (s.Count(ch => ch == '"') % 2 == 1)
        {
            int last = s.LastIndexOf('"');
            s = s.Remove(last, 1);
        }
        return Regex.Replace(s, @"[ ]{2,}", " ").Trim();
    }
}
