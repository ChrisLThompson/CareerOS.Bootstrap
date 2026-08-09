using CareerOS.Bootstrap.Models;

namespace CareerOS.Bootstrap.Services;

/// <summary>
/// Builds a list of directory paths from a CareerOS directory template
/// without modifying the filesystem.
/// </summary>
/// <remarks>
/// This service is intentionally read-only. It allows CareerOS.Bootstrap
/// to preview and validate a directory structure before actual provisioning
/// is enabled.
/// </remarks>
public class DirectoryPlanService
{
    /// <summary>
    /// Creates a complete directory plan for the supplied profile and template.
    /// </summary>
    /// <param name="basePath">
    /// The root path beneath which the profile directory would be created.
    /// </param>
    /// <param name="profile">
    /// The CareerOS profile being provisioned.
    /// </param>
    /// <param name="template">
    /// The resolved directory template assigned to the profile.
    /// </param>
    /// <returns>
    /// A read-only collection containing every directory path that would
    /// be created.
    /// </returns>
    public IReadOnlyList<string> BuildPlan(
        string basePath,
        ProfileConfiguration profile,
        CareerTemplate template)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new ArgumentException(
                "Base path cannot be empty.",
                nameof(basePath));
        }

        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(template);

        if (string.IsNullOrWhiteSpace(profile.Directory))
        {
            throw new InvalidOperationException(
                $"Profile '{profile.Name}' does not define a directory.");
        }

        List<string> paths = [];

        string profileRoot =
            Path.Combine(
                basePath,
                profile.Directory);

        paths.Add(profileRoot);

        foreach (DirectoryNode directory in template.Directories)
        {
            AddDirectoryNode(
                profileRoot,
                directory,
                paths);
        }

        return paths;
    }

    /// <summary>
    /// Recursively adds a directory node and all of its children
    /// to the provisioning plan.
    /// </summary>
    private static void AddDirectoryNode(
        string parentPath,
        DirectoryNode node,
        ICollection<string> paths)
    {
        if (string.IsNullOrWhiteSpace(node.Name))
        {
            throw new InvalidOperationException(
                "A directory template contains a directory with no name.");
        }

        string currentPath =
            Path.Combine(
                parentPath,
                node.Name);

        paths.Add(currentPath);

        foreach (DirectoryNode child in node.Children)
        {
            AddDirectoryNode(
                currentPath,
                child,
                paths);
        }
    }
}