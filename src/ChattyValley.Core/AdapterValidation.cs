namespace ChattyValley.Core;

/// <summary>
/// Cheap startup validation of an adapter GGUF: the file exists, is readable, and starts with the
/// GGUF magic. Deliberately NOT a full load (lazy-load design): this
/// catches missing and truncated-to-garbage files at launch so the handshake can report them,
/// while a file that passes here can still fail its real llama.cpp load on first use, which the
/// sidecar reports as adapter_load_failed.
/// </summary>
public static class AdapterValidation
{
    private static readonly byte[] GgufMagic = { 0x47, 0x47, 0x55, 0x46 }; // "GGUF"

    public static AdapterStatus Check(string name, string path)
    {
        if (!File.Exists(path))
            return new AdapterStatus(name, AdapterStatuses.MissingFile, path);
        try
        {
            using var fs = File.OpenRead(path);
            var head = new byte[4];
            int read = fs.Read(head, 0, 4);
            if (read < 4 || !head.AsSpan().SequenceEqual(GgufMagic))
                return new AdapterStatus(name, AdapterStatuses.NotGguf, path);
            return new AdapterStatus(name, AdapterStatuses.Ok);
        }
        catch (Exception ex)
        {
            return new AdapterStatus(name, AdapterStatuses.Unreadable, ex.Message);
        }
    }
}
