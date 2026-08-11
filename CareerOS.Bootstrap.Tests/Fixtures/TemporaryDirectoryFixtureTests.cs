using System.Text;

namespace CareerOS.Bootstrap.Tests.Fixtures;

public class TemporaryDirectoryFixtureTests
{
    [Fact]
    public void Constructor_CreatesUniqueTemporaryRoot()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        Assert.True(
            Directory.Exists(fixture.RootPath),
            $"Expected fixture root to exist: {fixture.RootPath}");

        Assert.True(
            Path.IsPathFullyQualified(fixture.RootPath),
            $"Expected an absolute fixture root but received: {fixture.RootPath}");
    }

    [Fact]
    public void Constructor_CreatesDifferentRootForEachFixtureInstance()
    {
        using TemporaryDirectoryFixture first =
            new();

        using TemporaryDirectoryFixture second =
            new();

        Assert.NotEqual(
            first.RootPath,
            second.RootPath);
    }

    [Fact]
    public void CreateDirectory_WithRelativeSegments_CreatesNestedDirectory()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string result =
            fixture.CreateDirectory(
                "Configuration",
                "Nested");

        string expected =
            Path.Combine(
                fixture.RootPath,
                "Configuration",
                "Nested");

        Assert.Equal(
            Path.GetFullPath(expected),
            result);

        Assert.True(
            Directory.Exists(result),
            $"Expected directory to exist: {result}");
    }

    [Fact]
    public void CreateFile_WithRelativePath_CreatesFileAndParentDirectories()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string result =
            fixture.CreateFile(
                Path.Combine(
                    "Configuration",
                    "bootstrap.json"),
                """
                {
                  "profiles": []
                }
                """);

        Assert.True(
            File.Exists(result),
            $"Expected file to exist: {result}");

        Assert.True(
            Directory.Exists(
                Path.GetDirectoryName(result)!));
    }

    [Fact]
    public void CreateFile_WritesExpectedUtf8Content()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        const string expected =
            "CareerOS — UTF-8 fixture content";

        string path =
            fixture.CreateFile(
                "utf8.txt",
                expected);

        string actual =
            File.ReadAllText(
                path,
                Encoding.UTF8);

        Assert.Equal(
            expected,
            actual);
    }

    [Fact]
    public void GetPath_WithRelativeSegments_ReturnsPathInsideFixtureRoot()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string result =
            fixture.GetPath(
                "Configuration",
                "templates.json");

        string expected =
            Path.Combine(
                fixture.RootPath,
                "Configuration",
                "templates.json");

        Assert.Equal(
            Path.GetFullPath(expected),
            result);

        Assert.StartsWith(
            EnsureTrailingSeparator(
                Path.GetFullPath(fixture.RootPath)),
            result,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetPath_DoesNotCreateFilesystemEntry()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string result =
            fixture.GetPath(
                "DoesNotExist",
                "file.txt");

        Assert.False(
            File.Exists(result));

        Assert.False(
            Directory.Exists(result));
    }

    [Fact]
    public void GetPath_WithAbsoluteSegment_ThrowsArgumentException()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string absolutePath =
            Path.GetFullPath(
                Path.Combine(
                    Path.GetTempPath(),
                    "outside-fixture"));

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => fixture.GetPath(
                    absolutePath));

        Assert.Equal(
            "pathSegments",
            exception.ParamName);

        Assert.Contains(
            "Fixture path segments must be relative.",
            exception.Message);
    }

    [Fact]
    public void GetPath_WithParentTraversalOutsideRoot_ThrowsInvalidOperationException()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => fixture.GetPath(
                    "..",
                    "outside-fixture"));

        Assert.Equal(
            "The requested fixture path escapes the temporary test root.",
            exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void GetPath_WithMissingSegment_ThrowsArgumentException(
        string segment)
    {
        using TemporaryDirectoryFixture fixture =
            new();

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => fixture.GetPath(
                    segment));

        Assert.Equal(
            "segment",
            exception.ParamName);
    }

    [Fact]
    public void GetPath_AfterDispose_ThrowsObjectDisposedException()
    {
        TemporaryDirectoryFixture fixture =
            new();

        fixture.Dispose();

        Assert.Throws<ObjectDisposedException>(
            () => fixture.GetPath(
                "Configuration"));
    }

    [Fact]
    public void CreateDirectory_AfterDispose_ThrowsObjectDisposedException()
    {
        TemporaryDirectoryFixture fixture =
            new();

        fixture.Dispose();

        Assert.Throws<ObjectDisposedException>(
            () => fixture.CreateDirectory(
                "Configuration"));
    }

    [Fact]
    public void CreateFile_AfterDispose_ThrowsObjectDisposedException()
    {
        TemporaryDirectoryFixture fixture =
            new();

        fixture.Dispose();

        Assert.Throws<ObjectDisposedException>(
            () => fixture.CreateFile(
                "test.txt",
                "test"));
    }

    [Fact]
    public void Dispose_RemovesFixtureRootAndContents()
    {
        TemporaryDirectoryFixture fixture =
            new();

        string rootPath =
            fixture.RootPath;

        string nestedDirectory =
            fixture.CreateDirectory(
                "Configuration");

        string filePath =
            fixture.CreateFile(
                Path.Combine(
                    "Configuration",
                    "bootstrap.json"),
                "{}");

        Assert.True(
            Directory.Exists(rootPath));

        Assert.True(
            Directory.Exists(nestedDirectory));

        Assert.True(
            File.Exists(filePath));

        fixture.Dispose();

        Assert.False(
            Directory.Exists(rootPath));
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_DoesNotThrow()
    {
        TemporaryDirectoryFixture fixture =
            new();

        fixture.Dispose();

        Exception? exception =
            Record.Exception(
                fixture.Dispose);

        Assert.Null(exception);
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
