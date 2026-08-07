using ChattyValley.Core;

namespace ChattyValley.Tests;

public class SidecarFailureMessagesTests
{
    [Theory]
    [InlineData(SidecarFailureKind.MissingFile)]
    [InlineData(SidecarFailureKind.LaunchFailed)]
    [InlineData(SidecarFailureKind.ExitedEarly)]
    [InlineData(SidecarFailureKind.ConnectTimeout)]
    public void EveryKind_ProducesHeadlineCauseAndDetails(SidecarFailureKind kind)
    {
        string[] lines = SidecarFailureMessages.Build(kind, "exit code 1");
        Assert.Equal(3, lines.Length);
        // DAT-681: headline says free chat is off and the game is unaffected.
        Assert.Contains("Free chat is OFF", lines[0]);
        Assert.Contains("unaffected", lines[0]);
        // Cause line carries the troubleshooting anchor.
        Assert.Contains(SidecarFailureMessages.TroubleshootingUrl, lines[1]);
        // Details line carries the underlying exception / exit code.
        Assert.StartsWith("Details: ", lines[2]);
        Assert.Contains("exit code 1", lines[2]);
    }

    [Fact]
    public void LaunchFailed_PointsAtWindowsBlocking()
    {
        string[] lines = SidecarFailureMessages.Build(SidecarFailureKind.LaunchFailed, "x");
        Assert.Contains("Windows blocking", lines[1]);
    }

    [Fact]
    public void MissingFile_TellsThePlayerToReinstall()
    {
        string[] lines = SidecarFailureMessages.Build(SidecarFailureKind.MissingFile, "base.gguf");
        Assert.Contains("Reinstall", lines[1]);
    }

    [Fact]
    public void Url_IsTheRepoTroubleshootingAnchor()
    {
        Assert.Equal("https://github.com/edbuildingstuff/chatty-valley#troubleshooting",
            SidecarFailureMessages.TroubleshootingUrl);
    }
}
