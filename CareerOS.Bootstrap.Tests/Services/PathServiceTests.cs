using CareerOS.Bootstrap.Services;
using CareerOS.Bootstrap.Tests.Fixtures;

namespace CareerOS.Bootstrap.Tests.Services;

public class PathServiceTests
{
    private readonly PathService _service = new();

    [Fact]
    public void FindRepositoryRoot_ReturnsDirectoryContainingSolutionFile()
    {
        string repositoryRoot =
            _service.FindRepositoryRoot();

        string solutionFile =
            Path.Combine(
                repositoryRoot,
                "CareerOS.Bootstrap.sln");

        Assert.True(
            Directory.Exists(repositoryRoot),
            $"Expected repository root to exist: {repositoryRoot}");

        Assert.True(
            File.Exists(solutionFile),
            $"Expected solution file to exist: {solutionFile}");
    }

    [Fact]
    public void FindRepositoryRoot_ReturnsAncestorOfApplicationBaseDirectory()
    {
        string repositoryRoot =
            Path.GetFullPath(
                _service.FindRepositoryRoot());

        string applicationBaseDirectory =
            Path.GetFullPath(
                AppContext.BaseDirectory);

        string normalizedRepositoryRoot =
            EnsureTrailingDirectorySeparator(repositoryRoot);

        string normalizedApplicationBaseDirectory =
            EnsureTrailingDirectorySeparator(applicationBaseDirectory);

        Assert.StartsWith(
            normalizedRepositoryRoot,
            normalizedApplicationBaseDirectory,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FindRepositoryRoot_ReturnsAbsolutePath()
    {
        string repositoryRoot =
            _service.FindRepositoryRoot();

        Assert.True(
            Path.IsPathFullyQualified(repositoryRoot),
            $"Expected an absolute repository path but received: {repositoryRoot}");
    }

    [Fact]
    public void FindRepositoryRoot_WithInjectedNestedStartDirectory_ReturnsContainingRepositoryRoot()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        fixture.CreateFile(
            "CareerOS.Bootstrap.sln",
            string.Empty);

        string nestedDirectory =
            fixture.CreateDirectory(
                "src",
                "nested",
                "bin");

        PathService service =
            new(nestedDirectory);

        string result =
            service.FindRepositoryRoot();

        Assert.Equal(
            Path.GetFullPath(fixture.RootPath),
            Path.GetFullPath(result));
    }

    [Fact]
    public void FindRepositoryRoot_WhenSolutionFileCannotBeFound_ThrowsDirectoryNotFoundException()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        PathService service =
            new(fixture.RootPath);

        DirectoryNotFoundException exception =
            Assert.Throws<DirectoryNotFoundException>(
                service.FindRepositoryRoot);

        Assert.Equal(
            "Unable to locate the CareerOS.Bootstrap repository root.",
            exception.Message);
    }

    [Fact]
    public void GetConfigurationDirectory_ReturnsConfigurationDirectoryUnderRepositoryRoot()
    {
        string repositoryRoot =
            _service.FindRepositoryRoot();

        string expected =
            Path.Combine(
                repositoryRoot,
                "Configuration");

        string result =
            _service.GetConfigurationDirectory();

        Assert.Equal(
            Path.GetFullPath(expected),
            Path.GetFullPath(result));
    }

    [Fact]
    public void GetConfigurationDirectory_ReturnsExistingDirectory()
    {
        string configurationDirectory =
            _service.GetConfigurationDirectory();

        Assert.True(
            Directory.Exists(configurationDirectory),
            $"Expected configuration directory to exist: {configurationDirectory}");
    }

    [Fact]
    public void GetConfigurationDirectory_ReturnsAbsolutePath()
    {
        string configurationDirectory =
            _service.GetConfigurationDirectory();

        Assert.True(
            Path.IsPathFullyQualified(configurationDirectory),
            $"Expected an absolute configuration path but received: {configurationDirectory}");
    }

    [Fact]
    public void GetConfigurationDirectory_ContainsCurrentConfigurationFiles()
    {
        string configurationDirectory =
            _service.GetConfigurationDirectory();

        string bootstrapConfiguration =
            Path.Combine(
                configurationDirectory,
                "bootstrap.json");

        string templateConfiguration =
            Path.Combine(
                configurationDirectory,
                "templates.json");

        Assert.True(
            File.Exists(bootstrapConfiguration),
            $"Expected bootstrap configuration file to exist: {bootstrapConfiguration}");

        Assert.True(
            File.Exists(templateConfiguration),
            $"Expected template configuration file to exist: {templateConfiguration}");
    }

    [Fact]
    public void GetConfigurationDirectory_WithInjectedRepositoryRoot_ReturnsInjectedConfigurationDirectory()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        fixture.CreateFile(
            "CareerOS.Bootstrap.sln",
            string.Empty);

        string configurationDirectory =
            fixture.CreateDirectory(
                "Configuration");

        string nestedStartDirectory =
            fixture.CreateDirectory(
                "src",
                "bin");

        PathService service =
            new(nestedStartDirectory);

        string result =
            service.GetConfigurationDirectory();

        Assert.Equal(
            Path.GetFullPath(configurationDirectory),
            Path.GetFullPath(result));
    }

    [Fact]
    public void GetConfigurationDirectory_WhenConfigurationDirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        fixture.CreateFile(
            "CareerOS.Bootstrap.sln",
            string.Empty);

        string nestedStartDirectory =
            fixture.CreateDirectory(
                "src",
                "bin");

        PathService service =
            new(nestedStartDirectory);

        string expectedConfigurationDirectory =
            Path.Combine(
                fixture.RootPath,
                "Configuration");

        DirectoryNotFoundException exception =
            Assert.Throws<DirectoryNotFoundException>(
                service.GetConfigurationDirectory);

        Assert.Equal(
            $"Configuration directory was not found: {expectedConfigurationDirectory}",
            exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Constructor_WithMissingStartDirectory_ThrowsArgumentException(
        string? startDirectory)
    {
        ArgumentException exception =
            Assert.ThrowsAny<ArgumentException>(
                () => new PathService(
                    startDirectory!));

        Assert.Equal(
            "startDirectory",
            exception.ParamName);
    }

    private static string EnsureTrailingDirectorySeparator(
        string path)
    {
        return path.EndsWith(
            Path.DirectorySeparatorChar.ToString(),
            StringComparison.Ordinal)
            ? path
            : path + Path.DirectorySeparatorChar;
    }
}
