using System.Text.Json;

namespace ChattyValley.Core;

/// <summary>A roster character plus its adapter GGUF resolved to a full path.</summary>
public sealed record RosterEntry(Character Character, string AdapterFullPath);

public sealed class RosterLoadResult
{
    public RosterLoadResult(IReadOnlyDictionary<string, RosterEntry> entries, IReadOnlyList<string> warnings)
    {
        Entries = entries; Warnings = warnings;
    }
    /// <summary>Keyed by character Name, which must equal the game's NPC name.</summary>
    public IReadOnlyDictionary<string, RosterEntry> Entries { get; }
    public IReadOnlyList<string> Warnings { get; }
}

/// <summary>
/// Scans characters/*.json into the villager roster. Forgiving by design: one broken file skips
/// that file with a warning and never takes the others down. Whether an adapter FILE is usable is
/// the sidecar's call (validation + handshake); this only resolves paths.
/// </summary>
public static class CharacterRoster
{
    public static RosterLoadResult Load(string charactersDir, string baseDir)
    {
        var entries = new Dictionary<string, RosterEntry>();
        var warnings = new List<string>();
        if (!Directory.Exists(charactersDir))
        {
            warnings.Add($"characters folder not found: {charactersDir}");
            return new RosterLoadResult(entries, warnings);
        }
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        foreach (string file in Directory.GetFiles(charactersDir, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            string leaf = Path.GetFileName(file);
            Character? c;
            try { c = JsonSerializer.Deserialize<Character>(File.ReadAllText(file), opts); }
            catch (Exception ex)
            {
                warnings.Add($"{leaf}: could not parse ({ex.Message}); skipped");
                continue;
            }
            if (c is null || string.IsNullOrWhiteSpace(c.Name))
            {
                warnings.Add($"{leaf}: has no Name; skipped");
                continue;
            }
            if (string.IsNullOrWhiteSpace(c.AdapterPath))
            {
                warnings.Add($"{leaf}: no adapterPath, so {c.Name} is not chattable; skipped");
                continue;
            }
            if (entries.ContainsKey(c.Name))
            {
                warnings.Add($"{leaf}: duplicate character name '{c.Name}'; first file wins, this one skipped");
                continue;
            }
            string full = Path.IsPathRooted(c.AdapterPath) ? c.AdapterPath : Path.GetFullPath(Path.Combine(baseDir, c.AdapterPath));
            entries[c.Name] = new RosterEntry(c, full);
        }
        return new RosterLoadResult(entries, warnings);
    }
}
