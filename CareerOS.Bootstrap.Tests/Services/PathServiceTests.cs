using CareerOS.Bootstrap.Services;

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
