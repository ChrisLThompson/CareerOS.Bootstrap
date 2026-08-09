namespace CareerOS.Bootstrap.Services;

public class PathService
{
    public string FindRepositoryRoot()
    {
        DirectoryInfo? currentDirectory =
            new(AppContext.BaseDirectory);

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