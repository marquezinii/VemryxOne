using System.IO;
using Ralven.Windows.Infrastructure;

namespace Ralven.App.Services;

/// <summary>
/// Imports user-owned state from unsupported product generations once.
/// Sources are never modified and executable/update state is intentionally excluded.
/// </summary>
internal static class LegacyDataImporter
{
    private const string MarkerName = ".legacy-data-import-v1";
    private static readonly string[] LegacyProductDirectories = ["Vemryx One", "FiveMCleaner"];
    private static readonly string[] AllowedFiles = ["settings.json", "firebase.session", "history.json"];
    private static readonly string[] AllowedDirectories = ["avatars", "Transactions", "AuthQuarantine"];
    private static readonly SafeFileTree FileTree = new();

    internal static void TryImport()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData)) return;

        TryImport(
            LegacyProductDirectories.Select(name => Path.Combine(localAppData, name)),
            Path.Combine(localAppData, "Ralven"));
    }

    internal static bool TryImport(IEnumerable<string> sourceRoots, string destinationRoot)
    {
        try
        {
            var destination = SafePath.EnsureNoReparsePoints(destinationRoot);
            var marker = Path.Combine(destination, MarkerName);
            if (File.Exists(marker)) return false;

            Directory.CreateDirectory(destination);
            SafePath.EnsureNoReparsePoints(destination);
            foreach (var sourceRoot in sourceRoots.Select(Path.GetFullPath))
            {
                if (!Directory.Exists(sourceRoot) || PathsEqual(sourceRoot, destination)) continue;
                try
                {
                    SafePath.EnsureNoReparsePoints(sourceRoot);
                }
                catch (IOException)
                {
                    continue;
                }

                foreach (var fileName in AllowedFiles)
                {
                    CopyFileIfMissing(
                        Path.Combine(sourceRoot, fileName),
                        Path.Combine(destination, fileName),
                        destination);
                }

                foreach (var directoryName in AllowedDirectories)
                {
                    CopyDirectoryIfMissing(
                        Path.Combine(sourceRoot, directoryName),
                        Path.Combine(destination, directoryName),
                        destination);
                }
            }

            SafePath.EnsureNoReparsePoints(destination);
            File.WriteAllText(marker, DateTimeOffset.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
            return true;
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException
            or NotSupportedException
            or System.Security.SecurityException
            or InvalidOperationException)
        {
            return false;
        }
    }

    private static void CopyDirectoryIfMissing(string source, string destination, string destinationRoot)
    {
        if (!Directory.Exists(source)) return;

        SafeFileEnumerationResult files;
        try
        {
            files = FileTree.EnumerateFiles(source, _ => true);
        }
        catch (IOException)
        {
            return;
        }

        foreach (var file in files.Files)
        {
            CopyFileIfMissing(
                file.FullPath,
                Path.Combine(destination, file.RelativePath),
                destinationRoot);
        }
    }

    private static void CopyFileIfMissing(string source, string destination, string destinationRoot)
    {
        if (!File.Exists(source)) return;

        try
        {
            source = SafePath.EnsureNoReparsePoints(source);
        }
        catch (IOException)
        {
            return;
        }

        destination = SafePath.EnsureDescendant(destinationRoot, destination);
        var destinationDirectory = Path.GetDirectoryName(destination)!;
        SafePath.EnsureNoReparsePoints(destinationDirectory);
        Directory.CreateDirectory(destinationDirectory);
        SafePath.EnsureNoReparsePoints(destinationDirectory);
        if (File.Exists(destination)) return;

        var temporary = destination + $".{Guid.NewGuid():N}.importing";
        try
        {
            SafePath.EnsureNoReparsePoints(source);
            File.Copy(source, temporary, overwrite: false);
            SafePath.EnsureNoReparsePoints(destinationDirectory);
            File.Move(temporary, destination, overwrite: false);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static bool PathsEqual(string left, string right) =>
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)).Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
            StringComparison.OrdinalIgnoreCase);
}
