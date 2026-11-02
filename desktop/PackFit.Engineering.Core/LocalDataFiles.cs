using System.Text;

namespace PackFit.Engineering.Core;

/// <summary>
/// Reads user-selected local reports with a strict byte budget before parsing.
/// A file growing after its initial size check cannot consume unbounded RAM.
/// No network access, elevation or telemetry is performed.
/// </summary>
public static class LocalDataFiles
{
    public static async Task<string> ReadTextAsync(
        string path, int maxBytes, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Select a file.", nameof(path));
        if (maxBytes <= 0 || maxBytes > 20_000_000)
            throw new ArgumentOutOfRangeException(nameof(maxBytes));

        await using var file = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.Read, 65536, FileOptions.Asynchronous | FileOptions.SequentialScan);
        if (file.Length > maxBytes)
            throw new FormatException($"Local file exceeds the {maxBytes:N0}-byte import limit.");

        using var buffer = new MemoryStream();
        var chunk = new byte[65536];
        int bytesRead = 0;
        while (true)
        {
            int request = Math.Min(chunk.Length, maxBytes + 1 - bytesRead);
            if (request <= 0)
                throw new FormatException($"Local file exceeds the {maxBytes:N0}-byte import limit.");
            int read = await file.ReadAsync(chunk.AsMemory(0, request), cancellationToken);
            if (read == 0) break;
            bytesRead += read;
            if (bytesRead > maxBytes)
                throw new FormatException($"Local file exceeds the {maxBytes:N0}-byte import limit.");
            buffer.Write(chunk, 0, read);
        }

        buffer.Position = 0;
        using var reader = new StreamReader(buffer, Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
