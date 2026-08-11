namespace CareerOS.Bootstrap.Tests.Fixtures;

/// <summary>
/// Provides an isolated temporary filesystem root for tests that need
/// real filesystem operations without touching a user workspace.
/// </summary>
public sealed class TemporaryDirectoryFixture : IDisposable
{
    private bool _disposed;

    public TemporaryDirectoryFixture()
    {
        RootPath = Path.Combine(
            Path.GetTempPath(),
            "CareerOS.Bootstrap.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(RootPath);
    }

    /// <summary>
    /// Gets the unique temporary root owned by this fixture instance.
    /// </summary>
    public string RootPath { get; }

    /// <summary>
    /// Creates and returns a directory beneath the fixture root.
    /// </summary>
    public string CreateDirectory(
        params string[] pathSegments)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        string path = CombineUnderRoot(pathSegments);

        Directory.CreateDirectory(path);

        return path;
    }

    /// <summary>
    /// Creates a UTF-8 text file beneath the fixture root and returns
    /// the resulting absolute path.
    /// </summary>
    public string CreateFile(
        string relativePath,
        string contents)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        string path = CombineUnderRoot(relativePath);

        string? parentDirectory =
            Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(parentDirectory))
        {
            Directory.CreateDirectory(parentDirectory);
        }

        File.WriteAllText(
            path,
            contents);

        return path;
    }

    /// <summary>
    /// Combines one or more relative path segments beneath the fixture root.
    /// The returned path is validated to remain inside the fixture boundary.
    /// </summary>
    public string GetPath(
        params string[] pathSegments)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        return CombineUnderRoot(pathSegments);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (Directory.Exists(RootPath))
        {
            Directory.Delete(
                RootPath,
                recursive: true);
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private string CombineUnderRoot(
        params string[] pathSegments)
    {
        ArgumentNullException.ThrowIfNull(pathSegments);

        string combinedPath = RootPath;

        foreach (string segment in pathSegments)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(segment);

            if (Path.IsPathFullyQualified(segment))
            {
                throw new ArgumentException(
                    "Fixture path segments must be relative.",
                    nameof(pathSegments));
            }

            combinedPath =
                Path.Combine(
                    combinedPath,
                    segment);
        }

        string rootFullPath =
            EnsureTrailingSeparator(
                Path.GetFullPath(RootPath));

        string candidateFullPath =
            Path.GetFullPath(combinedPath);

        if (!candidateFullPath.StartsWith(
                rootFullPath,
                StringComparison.OrdinalIgnoreCase)
            && !string.Equals(
                candidateFullPath,
                RootPath,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The requested fixture path escapes the temporary test root.");
        }

        return candidateFullPath;
    }

    private static string EnsureTrailingSeparator(
        string path)
    {
        return path.EndsWith(
            Path.DirectorySeparatorChar.ToString(),
            StringComparison.Ordinal)
            ? path
            : path + Path.DirectorySeparatorChar;
    }
}
