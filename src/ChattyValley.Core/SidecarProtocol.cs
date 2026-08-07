using System.Text.Json;

namespace ChattyValley.Core;

/// <summary>
/// The wire contract between the mod and the sidecar: one JSON object per line over the named
/// pipe, camelCase property names. The sidecar sends one <see cref="SidecarHandshake"/> line when
/// the client connects, then answers each <see cref="SidecarRequest"/> line with either
/// {"reply","raw"} or {"error",...}. Mod and sidecar ship together in one zip, so there is no
/// cross-version compatibility layer.
/// </summary>
public static class SidecarJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };
}

/// <summary>Error codes the sidecar returns in {"error": ...} responses.</summary>
public static class SidecarErrors
{
    public const string UnknownCharacter = "unknown_character";
    public const string AdapterLoadFailed = "adapter_load_failed";
    public const string BadRequest = "bad_request";
}

/// <summary>Adapter validation states reported in the handshake.</summary>
public static class AdapterStatuses
{
    public const string Ok = "ok";
    public const string MissingFile = "missing_file";
    public const string NotGguf = "not_gguf";
    public const string Unreadable = "unreadable";
}

public sealed record SidecarRequest(
    string? Prompt, string? Character, float Temp, int MaxTokens,
    float? RepeatPenalty, float? FrequencyPenalty);

public sealed record AdapterStatus(string Name, string Status, string? Detail = null);

public sealed record SidecarHandshake(bool Ready, string Base, IReadOnlyList<AdapterStatus> Adapters);

/// <summary>Parses one repeatable sidecar CLI argument of the form <c>name=path</c>.</summary>
public static class AdapterArg
{
    /// <summary>Split on the FIRST '=' so Windows paths containing '=' survive.</summary>
    public static bool TryParse(string? arg, out string name, out string path)
    {
        name = ""; path = "";
        if (string.IsNullOrEmpty(arg)) return false;
        int i = arg.IndexOf('=');
        if (i <= 0 || i == arg.Length - 1) return false;
        name = arg.Substring(0, i);
        path = arg.Substring(i + 1);
        return true;
    }
}
