using System.Text.Json;
using ChattyValley.Core;

namespace ChattyValley.Tests;

public class SidecarProtocolTests
{
    [Fact]
    public void Request_RoundTrips_CamelCaseOnTheWire()
    {
        var req = new SidecarRequest("<prompt>", "Linus", 0.35f, 96, 1.1f, 0.1f);
        string json = JsonSerializer.Serialize(req, SidecarJson.Options);
        Assert.Contains("\"prompt\"", json);
        Assert.Contains("\"character\"", json);
        Assert.Contains("\"maxTokens\"", json);
        var back = JsonSerializer.Deserialize<SidecarRequest>(json, SidecarJson.Options)!;
        Assert.Equal("Linus", back.Character);
        Assert.Equal(96, back.MaxTokens);
    }

    [Fact]
    public void Request_ReadsLegacyShapeWithoutCharacter_AsNullCharacter()
    {
        // The old mod serialized {prompt,temp,maxTokens,...} with no character. The sidecar must
        // parse it (and then reject it as bad_request at the routing layer, not crash here).
        var back = JsonSerializer.Deserialize<SidecarRequest>(
            "{\"prompt\":\"p\",\"temp\":0.35,\"maxTokens\":96}", SidecarJson.Options)!;
        Assert.Null(back.Character);
        Assert.Equal("p", back.Prompt);
    }

    [Fact]
    public void Handshake_RoundTrips()
    {
        var hs = new SidecarHandshake(true, "base.gguf", new[]
        {
            new AdapterStatus("Linus", AdapterStatuses.Ok),
            new AdapterStatus("Pierre", AdapterStatuses.MissingFile, "no such file"),
        });
        string json = JsonSerializer.Serialize(hs, SidecarJson.Options);
        var back = JsonSerializer.Deserialize<SidecarHandshake>(json, SidecarJson.Options)!;
        Assert.True(back.Ready);
        Assert.Equal(2, back.Adapters.Count);
        Assert.Equal(AdapterStatuses.MissingFile, back.Adapters[1].Status);
        Assert.Equal("no such file", back.Adapters[1].Detail);
    }

    [Theory]
    [InlineData("Linus=C:\\models\\linus.gguf", "Linus", "C:\\models\\linus.gguf")]
    [InlineData("Linus=C:\\odd=name.gguf", "Linus", "C:\\odd=name.gguf")] // split on FIRST '=' only
    public void AdapterArg_Parses(string arg, string expectName, string expectPath)
    {
        Assert.True(AdapterArg.TryParse(arg, out var name, out var path));
        Assert.Equal(expectName, name);
        Assert.Equal(expectPath, path);
    }

    [Theory]
    [InlineData("noequals")]
    [InlineData("=pathonly")]
    [InlineData("nameonly=")]
    [InlineData("")]
    public void AdapterArg_RejectsMalformed(string arg)
    {
        Assert.False(AdapterArg.TryParse(arg, out _, out _));
    }
}
