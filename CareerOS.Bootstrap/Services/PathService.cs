namespace CareerOS.Bootstrap.Services;

public class PathService
{
    private readonly string _startDirectory;

    public PathService()
        : this(AppContext.BaseDirectory)
    {
    }

    public PathService(string startDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(startDirectory);

        _startDirectory =
            Path.GetFullPath(startDirectory);
    }

    public string FindRepositoryRoot()
    {
        DirectoryInfo? currentDirectory =
            new(_startDirectory);

        while (currentDirectory != null)
        {
            string solutionFile =
                Path.Combine(
                    currentDirectory.FullName,
                    "CareerOS.Bootstrap.sln");

            if (File.Exists(solutionFile))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Unable to locate the CareerOS.Bootstrap repository root.");
    }

    public string GetConfigurationDirectory()
    {
        string repositoryRoot = FindRepositoryRoot();

        string configurationDirectory =
            Path.Combine(
                repositoryRoot,
                "Configuration");

        if (!Directory.Exists(configurationDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Configuration directory was not found: {configurationDirectory}");
        }

        return configurationDirectory;
    }
}
