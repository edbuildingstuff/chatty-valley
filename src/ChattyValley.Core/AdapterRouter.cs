namespace ChattyValley.Core;

/// <summary>
/// Decides whether a request's character can be served. Only adapters that passed startup
/// validation are routable; a first-use load failure is recorded once and every later request for
/// that character fails fast with the same adapter_load_failed, so a broken adapter costs one
/// llama.cpp load attempt per session, never one per turn.
/// </summary>
public sealed class AdapterRouter
{
    private readonly HashSet<string> _routable;
    private readonly Dictionary<string, string> _loadFailed = new();

    public AdapterRouter(IEnumerable<AdapterStatus> statuses) =>
        _routable = statuses.Where(s => s.Status == AdapterStatuses.Ok)
                            .Select(s => s.Name)
                            .ToHashSet();

    public bool TryRoute(string? character, out string errorCode, out string? errorDetail)
    {
        errorCode = ""; errorDetail = null;
        if (string.IsNullOrEmpty(character)) { errorCode = SidecarErrors.BadRequest; return false; }
        if (_loadFailed.TryGetValue(character, out var detail))
        {
            errorCode = SidecarErrors.AdapterLoadFailed; errorDetail = detail; return false;
        }
        if (!_routable.Contains(character)) { errorCode = SidecarErrors.UnknownCharacter; return false; }
        return true;
    }

    public void MarkLoadFailed(string character, string detail)
    {
        _routable.Remove(character);
        _loadFailed[character] = detail;
    }
}
