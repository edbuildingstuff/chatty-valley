using ChattyValley.Core;

namespace ChattyValley.Tests;

public class AdapterRouterTests
{
    private static AdapterRouter Router() => new(new[]
    {
        new AdapterStatus("Linus", AdapterStatuses.Ok),
        new AdapterStatus("Pierre", AdapterStatuses.MissingFile, "no file"),
    });

    [Fact]
    public void ValidatedCharacter_Routes()
    {
        Assert.True(Router().TryRoute("Linus", out _, out _));
    }

    [Fact]
    public void MissingCharacterField_IsBadRequest()
    {
        var r = Router();
        Assert.False(r.TryRoute(null, out var code, out _));
        Assert.Equal(SidecarErrors.BadRequest, code);
        Assert.False(r.TryRoute("", out code, out _));
        Assert.Equal(SidecarErrors.BadRequest, code);
    }

    [Fact]
    public void UnregisteredOrFailedValidation_IsUnknownCharacter()
    {
        var r = Router();
        Assert.False(r.TryRoute("Abigail", out var code, out _));
        Assert.Equal(SidecarErrors.UnknownCharacter, code);
        // Pierre failed validation, so he was never registered with the runtime either.
        Assert.False(r.TryRoute("Pierre", out code, out _));
        Assert.Equal(SidecarErrors.UnknownCharacter, code);
    }

    [Fact]
    public void LoadFailure_IsRemembered_AndFailsFast()
    {
        var r = Router();
        r.MarkLoadFailed("Linus", "llama_adapter_lora_init failed");
        Assert.False(r.TryRoute("Linus", out var code, out var detail));
        Assert.Equal(SidecarErrors.AdapterLoadFailed, code);
        Assert.Equal("llama_adapter_lora_init failed", detail);
    }
}
