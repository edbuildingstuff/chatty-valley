namespace ChattyValley.Core;

/// <summary>The startup failure classes the mod can tell apart (DAT-681).</summary>
public enum SidecarFailureKind
{
    /// <summary>A required file (base GGUF, sidecar exe) is missing before launch was attempted.</summary>
    MissingFile,
    /// <summary>Process.Start itself failed. On a player machine this is usually security policy.</summary>
    LaunchFailed,
    /// <summary>The process started and then exited before the pipe handshake.</summary>
    ExitedEarly,
    /// <summary>The process is running but the pipe never connected or never handshook.</summary>
    ConnectTimeout,
}

/// <summary>
/// The three SMAPI console lines for a sidecar startup failure, per DAT-681: headline first
/// (free chat off, game unaffected), likely cause in player language with the troubleshooting
/// link, then the technical detail for the players who can act on it. SMAPI copies these into
/// SMAPI-latest.txt, which is what ends up in bug reports.
/// </summary>
public static class SidecarFailureMessages
{
    public const string TroubleshootingUrl =
        "https://github.com/edbuildingstuff/chatty-valley#troubleshooting";

    public static string[] Build(SidecarFailureKind kind, string detail)
    {
        string cause = kind switch
        {
            SidecarFailureKind.MissingFile =>
                $"A file the mod needs is missing. Reinstall the ChattyValley folder from the zip. See {TroubleshootingUrl}",
            SidecarFailureKind.LaunchFailed =>
                $"This is usually Windows blocking an unsigned program. See {TroubleshootingUrl}",
            SidecarFailureKind.ExitedEarly =>
                $"The AI helper closed right after starting. This is usually Windows or an antivirus blocking it. See {TroubleshootingUrl}",
            SidecarFailureKind.ConnectTimeout =>
                $"The AI helper started but never answered. See {TroubleshootingUrl}",
            _ => TroubleshootingUrl,
        };
        return new[]
        {
            "Free chat is OFF: the AI helper program could not start. The rest of the game is unaffected.",
            cause,
            $"Details: {detail}",
        };
    }
}
