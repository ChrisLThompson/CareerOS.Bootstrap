using CareerOS.Bootstrap.Models;

namespace CareerOS.Bootstrap.Services;

public sealed class ConfigurationValidationService
{
    private static readonly HashSet<string> ReservedWindowsNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "CON",
            "PRN",
            "AUX",
            "NUL",
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "COM9",
            "LPT1",
            "LPT2",
            "LPT3",
            "LPT4",
            "LPT5",
            "LPT6",
            "LPT7",
            "LPT8",
            "LPT9"
        };

    public ValidationResult Validate(
        BootstrapConfiguration bootstrapConfiguration,
        TemplateConfiguration templateConfiguration)
    {
        ArgumentNullException.ThrowIfNull(bootstrapConfiguration);
        ArgumentNullException.ThrowIfNull(templateConfiguration);

        ValidationResult result = new();

        ValidateDestinationRootCore(
            bootstrapConfiguration.DestinationRoot,
            result);

        ValidateProfiles(
            bootstrapConfiguration,
            result);

        ValidateTemplates(
            templateConfiguration,
            result);

        ValidateTemplateReferences(
            bootstrapConfiguration,
            templateConfiguration,
            result);

        return result;
    }

    public ValidationResult ValidateDestinationRoot(
        string destinationRoot)
    {
        ValidationResult result = new();

        ValidateDestinationRootCore(
            destinationRoot,
            result);

        return result;
    }

    public ValidationResult ValidatePlannedPaths(
        string destinationRoot,
        IEnumerable<string> plannedPaths)
    {
        ArgumentNullException.ThrowIfNull(plannedPaths);

        ValidationResult result = new();

        string? normalizedRoot =
            ValidateDestinationRootCore(
                destinationRoot,
                result);

        if (normalizedRoot is null)
        {
            return result;
        }

        int index = 0;

        foreach (string plannedPath in plannedPaths)
        {
            string propertyName =
                $"PlannedPaths[{index}]";

            if (string.IsNullOrWhiteSpace(plannedPath))
            {
                result.AddError(
                    "PLANNED_PATH_REQUIRED",
                    "Planned path is required.",
                    propertyName);

                index++;
                continue;
            }

            if (!Path.IsPathFullyQualified(plannedPath))
            {
                result.AddError(
                    "PLANNED_PATH_NOT_FULLY_QUALIFIED",
                    $"Planned path '{plannedPath}' must be fully qualified.",
                    propertyName);

                index++;
                continue;
            }

            string normalizedPath;

            try
            {
                normalizedPath =
                    Path.GetFullPath(plannedPath);
            }
            catch (Exception exception)
                when (exception is ArgumentException or
                      NotSupportedException or
                      PathTooLongException)
            {
                result.AddError(
                    "PLANNED_PATH_INVALID",
                    $"Planned path '{plannedPath}' is invalid.",
                    propertyName);

                index++;
                continue;
            }

            if (!IsPathWithinRoot(
                    normalizedRoot,
                    normalizedPath))
            {
                result.AddError(
                    "PLANNED_PATH_OUTSIDE_DESTINATION_ROOT",
                    $"Planned path '{plannedPath}' is outside the approved destination root '{destinationRoot}'.",
                    propertyName);
            }

            index++;
        }

        return result;
    }

    private static string? ValidateDestinationRootCore(
        string destinationRoot,
        ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(destinationRoot))
        {
            result.AddError(
                "DESTINATION_ROOT_REQUIRED",
                "Destination root is required.",
                "DestinationRoot");

            return null;
        }

        if (!Path.IsPathFullyQualified(destinationRoot))
        {
            result.AddError(
                "DESTINATION_ROOT_NOT_FULLY_QUALIFIED",
                $"Destination root '{destinationRoot}' must be fully qualified.",
                "DestinationRoot");

            return null;
        }

        string normalizedRoot;

        try
        {
            normalizedRoot =
                Path.GetFullPath(destinationRoot);
        }
        catch (Exception exception)
            when (exception is ArgumentException or
                  NotSupportedException or
                  PathTooLongException)
        {
            result.AddError(
                "DESTINATION_ROOT_INVALID",
                $"Destination root '{destinationRoot}' is invalid.",
                "DestinationRoot");

            return null;
        }

        if (ContainsInvalidDestinationPathSegment(
                normalizedRoot))
        {
            result.AddError(
                "DESTINATION_ROOT_INVALID",
                $"Destination root '{destinationRoot}' contains an invalid or reserved directory name.",
                "DestinationRoot");

            return null;
        }

        return normalizedRoot;
    }

    private static bool ContainsInvalidDestinationPathSegment(
        string path)
    {
        string root =
            Path.GetPathRoot(path) ?? string.Empty;

        string remainder =
            path[root.Length..];

        string[] segments =
            remainder.Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries);

        return segments.Any(
            segment =>
                ContainsInvalidWindowsFilenameCharacter(segment) ||
                segment.EndsWith(' ') ||
                segment.EndsWith('.') ||
                IsReservedWindowsName(segment));
    }

    private static bool IsPathWithinRoot(
        string normalizedRoot,
        string normalizedPath)
    {
        string root =
            Path.TrimEndingDirectorySeparator(
                normalizedRoot);

        string path =
            Path.TrimEndingDirectorySeparator(
                normalizedPath);

        if (string.Equals(
                root,
                path,
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string rootWithSeparator =
            root + Path.DirectorySeparatorChar;

        return path.StartsWith(
            rootWithSeparator,
            StringComparison.OrdinalIgnoreCase);
    }

    private static void ValidateProfiles(
        BootstrapConfiguration configuration,
        ValidationResult result)
    {
        if (configuration.Profiles.Count == 0)
        {
            result.AddError(
                "PROFILE_COLLECTION_EMPTY",
                "At least one profile must be configured.",
                "Profiles");

            return;
        }

        for (int index = 0; index < configuration.Profiles.Count; index++)
        {
            ProfileConfiguration profile =
                configuration.Profiles[index];

            string prefix =
                $"Profiles[{index}]";

            if (string.IsNullOrWhiteSpace(profile.Name))
            {
                result.AddError(
                    "PROFILE_NAME_REQUIRED",
                    "Profile name is required.",
                    $"{prefix}.Name");
            }

            if (string.IsNullOrWhiteSpace(profile.Directory))
            {
                result.AddError(
                    "PROFILE_DIRECTORY_REQUIRED",
                    "Profile directory is required.",
                    $"{prefix}.Directory");
            }
            else
            {
                ValidateFilesystemName(
                    profile.Directory,
                    $"{prefix}.Directory",
                    "PROFILE_DIRECTORY_INVALID",
                    "PROFILE_DIRECTORY_RESERVED",
                    result);
            }

            if (string.IsNullOrWhiteSpace(profile.Template))
            {
                result.AddError(
                    "PROFILE_TEMPLATE_REQUIRED",
                    "Profile template is required.",
                    $"{prefix}.Template");
            }
        }

        AddDuplicateProfileNameErrors(
            configuration,
            result);

        AddDuplicateProfileDirectoryErrors(
            configuration,
            result);
    }

    private static void ValidateTemplates(
        TemplateConfiguration configuration,
        ValidationResult result)
    {
        if (configuration.Templates.Count == 0)
        {
            result.AddError(
                "TEMPLATE_COLLECTION_EMPTY",
                "At least one template must be configured.",
                "Templates");

            return;
        }

        for (int index = 0; index < configuration.Templates.Count; index++)
        {
            CareerTemplate template =
                configuration.Templates[index];

            string prefix =
                $"Templates[{index}]";

            if (string.IsNullOrWhiteSpace(template.Name))
            {
                result.AddError(
                    "TEMPLATE_NAME_REQUIRED",
                    "Template name is required.",
                    $"{prefix}.Name");
            }

            ValidateDirectoryNodes(
                template.Directories,
                $"{prefix}.Directories",
                result);
        }

        AddDuplicateTemplateNameErrors(
            configuration,
            result);
    }

    private static void ValidateTemplateReferences(
        BootstrapConfiguration bootstrapConfiguration,
        TemplateConfiguration templateConfiguration,
        ValidationResult result)
    {
        HashSet<string> templateNames =
            templateConfiguration.Templates
                .Where(template => !string.IsNullOrWhiteSpace(template.Name))
                .Select(template => template.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (int index = 0; index < bootstrapConfiguration.Profiles.Count; index++)
        {
            ProfileConfiguration profile =
                bootstrapConfiguration.Profiles[index];

            if (string.IsNullOrWhiteSpace(profile.Template))
            {
                continue;
            }

            if (!templateNames.Contains(profile.Template))
            {
                result.AddError(
                    "PROFILE_TEMPLATE_NOT_FOUND",
                    $"Profile '{DisplayProfileName(profile, index)}' references template '{profile.Template}', but that template is not configured.",
                    $"Profiles[{index}].Template");
            }
        }
    }

    private static void ValidateDirectoryNodes(
        IReadOnlyList<DirectoryNode> nodes,
        string collectionPath,
        ValidationResult result)
    {
        Dictionary<string, int> firstIndexByName =
            new(StringComparer.OrdinalIgnoreCase);

        for (int index = 0; index < nodes.Count; index++)
        {
            DirectoryNode node =
                nodes[index];

            string nodePath =
                $"{collectionPath}[{index}]";

            if (string.IsNullOrWhiteSpace(node.Name))
            {
                result.AddError(
                    "DIRECTORY_NAME_REQUIRED",
                    "Directory name is required.",
                    $"{nodePath}.Name");
            }
            else
            {
                ValidateFilesystemName(
                    node.Name,
                    $"{nodePath}.Name",
                    "DIRECTORY_NAME_INVALID",
                    "DIRECTORY_NAME_RESERVED",
                    result);

                if (!firstIndexByName.TryAdd(node.Name, index))
                {
                    result.AddError(
                        "DUPLICATE_SIBLING_DIRECTORY",
                        $"Directory name '{node.Name}' is duplicated within the same parent directory.",
                        $"{nodePath}.Name");
                }
            }

            ValidateDirectoryNodes(
                node.Children,
                $"{nodePath}.Children",
                result);
        }
    }

    private static void ValidateFilesystemName(
        string name,
        string propertyName,
        string invalidCode,
        string reservedCode,
        ValidationResult result)
    {
        if (ContainsInvalidWindowsFilenameCharacter(name) ||
            name.EndsWith(' ') ||
            name.EndsWith('.'))
        {
            result.AddError(
                invalidCode,
                $"'{name}' is not a valid directory name.",
                propertyName);
        }

        if (IsReservedWindowsName(name))
        {
            result.AddError(
                reservedCode,
                $"'{name}' is a reserved filesystem name.",
                propertyName);
        }
    }

    private static bool ContainsInvalidWindowsFilenameCharacter(
        string value)
    {
        return value.IndexOfAny(
            ['<', '>', ':', '"', '/', '\\', '|', '?', '*']) >= 0 ||
            value.Any(character => character < 32);
    }

    private static bool IsReservedWindowsName(
        string value)
    {
        string trimmed =
            value.TrimEnd(' ', '.');

        string baseName =
            Path.GetFileNameWithoutExtension(trimmed);

        return ReservedWindowsNames.Contains(baseName);
    }

    private static void AddDuplicateProfileNameErrors(
        BootstrapConfiguration configuration,
        ValidationResult result)
    {
        Dictionary<string, int> firstIndexByName =
            new(StringComparer.OrdinalIgnoreCase);

        for (int index = 0; index < configuration.Profiles.Count; index++)
        {
            string name =
                configuration.Profiles[index].Name;

            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (firstIndexByName.TryAdd(name, index))
            {
                continue;
            }

            result.AddError(
                "DUPLICATE_PROFILE_NAME",
                $"Profile name '{name}' is configured more than once.",
                $"Profiles[{index}].Name");
        }
    }

    private static void AddDuplicateProfileDirectoryErrors(
        BootstrapConfiguration configuration,
        ValidationResult result)
    {
        Dictionary<string, int> firstIndexByDirectory =
            new(StringComparer.OrdinalIgnoreCase);

        for (int index = 0; index < configuration.Profiles.Count; index++)
        {
            string directory =
                configuration.Profiles[index].Directory;

            if (string.IsNullOrWhiteSpace(directory))
            {
                continue;
            }

            if (firstIndexByDirectory.TryAdd(directory, index))
            {
                continue;
            }

            result.AddError(
                "DUPLICATE_PROFILE_DIRECTORY",
                $"Profile directory '{directory}' is configured more than once.",
                $"Profiles[{index}].Directory");
        }
    }

    private static void AddDuplicateTemplateNameErrors(
        TemplateConfiguration configuration,
        ValidationResult result)
    {
        Dictionary<string, int> firstIndexByName =
            new(StringComparer.OrdinalIgnoreCase);

        for (int index = 0; index < configuration.Templates.Count; index++)
        {
            string name =
                configuration.Templates[index].Name;

            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (firstIndexByName.TryAdd(name, index))
            {
                continue;
            }

            result.AddError(
                "DUPLICATE_TEMPLATE_NAME",
                $"Template name '{name}' is configured more than once.",
                $"Templates[{index}].Name");
        }
    }

    private static string DisplayProfileName(
        ProfileConfiguration profile,
        int index)
    {
        return string.IsNullOrWhiteSpace(profile.Name)
            ? $"#{index + 1}"
            : profile.Name;
    }
}
