using System.Text;

namespace ScreenExposure.Core;

public static class IccProfileImport
{
    private const int IccHeaderSize = 128;
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".icc",
        ".icm"
    };

    public static IccImportCandidate ValidateImportCandidate(string sourcePath, byte[] profileBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentNullException.ThrowIfNull(profileBytes);

        var extension = Path.GetExtension(sourcePath);
        if (!SupportedExtensions.Contains(extension))
        {
            throw new ArgumentException("ICC import only supports .icc and .icm files.", nameof(sourcePath));
        }

        if (profileBytes.Length < IccHeaderSize)
        {
            throw new ArgumentException("ICC profile is too small to contain a valid header.", nameof(profileBytes));
        }

        var signature = Encoding.ASCII.GetString(profileBytes, 36, 4);
        if (signature != "acsp")
        {
            throw new ArgumentException("ICC profile does not contain the acsp signature.", nameof(profileBytes));
        }

        return new IccImportCandidate(
            FileName: Path.GetFileName(sourcePath),
            Extension: extension.ToLowerInvariant());
    }

    public static string SanitizeFileName(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var invalidCharacters = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName
            .Select(character => invalidCharacters.Contains(character) ? '_' : character)
            .ToArray());

        return string.IsNullOrWhiteSpace(sanitized) ? "imported-profile.icc" : sanitized;
    }
}

public sealed record IccImportCandidate(string FileName, string Extension);
