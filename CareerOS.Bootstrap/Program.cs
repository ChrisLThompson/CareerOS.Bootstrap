using CareerOS.Bootstrap.Models;
using CareerOS.Bootstrap.Services;

namespace CareerOS.Bootstrap;

internal class Program
{
    private static int Main(string[] args)
    {
        PathService pathService = new();
        JsonConfigurationService jsonService = new();
        TemplateResolverService templateResolver = new();
        DirectoryPlanService directoryPlanService = new();
        ConfigurationValidationService validationService = new();
        ProvisioningPlanService provisioningPlanService = new();

        try
        {
            string repositoryRoot =
                pathService.FindRepositoryRoot();

            string configurationDirectory =
                pathService.GetConfigurationDirectory();

            string bootstrapPath =
                Path.Combine(
                    configurationDirectory,
                    "bootstrap.json");

            string templatesPath =
                Path.Combine(
                    configurationDirectory,
                    "templates.json");

            var bootstrapConfiguration =
                jsonService.LoadBootstrapConfiguration(
                    bootstrapPath);

            var templateConfiguration =
                jsonService.LoadTemplateConfiguration(
                    templatesPath);

            var configurationValidation =
                validationService.Validate(
                    bootstrapConfiguration,
                    templateConfiguration);

            if (!configurationValidation.IsValid)
            {
                return WriteValidationFailure(
                    "Configuration validation failed.",
                    configurationValidation);
            }

            Console.WriteLine();
            Console.WriteLine("CareerOS Bootstrap");
            Console.WriteLine("==================");
            Console.WriteLine();

            Console.WriteLine("Repository:");
            Console.WriteLine($"  {repositoryRoot}");
            Console.WriteLine();

            Console.WriteLine("Configuration:");
            Console.WriteLine($"  {configurationDirectory}");
            Console.WriteLine();

            Console.WriteLine("DRY RUN");
            Console.WriteLine("No directories will be created.");
            Console.WriteLine();

            /*
             * TEMPORARY PREVIEW ROOT
             *
             * This path exists only so we can validate template traversal.
             *
             * FUTURE:
             * The destination root will be supplied by bootstrap.json
             * and/or a command-line option.
             *
             * Do not use this value for actual directory creation.
             */
            string previewRoot =
                Path.Combine(
                    repositoryRoot,
                    "_Preview");

            var destinationValidation =
                validationService.ValidateDestinationRoot(
                    previewRoot);

            if (!destinationValidation.IsValid)
            {
                return WriteValidationFailure(
                    "Destination validation failed.",
                    destinationValidation);
            }

            foreach (var profile in bootstrapConfiguration.Profiles)
            {
                var template =
                    templateResolver.ResolveTemplate(
                        templateConfiguration,
                        profile.Template);

                IReadOnlyList<string> directoryPlan =
                    directoryPlanService.BuildPlan(
                        previewRoot,
                        profile,
                        template);

                var planValidation =
                    validationService.ValidatePlannedPaths(
                        previewRoot,
                        directoryPlan);

                if (!planValidation.IsValid)
                {
                    return WriteValidationFailure(
                        $"Planned-path validation failed for profile '{profile.Name}'.",
                        planValidation);
                }

                ProvisioningPlan provisioningPlan =
                    provisioningPlanService.BuildPlan(
                        directoryPlan);

                Console.WriteLine($"Profile: {profile.Name}");
                Console.WriteLine($"Template: {template.Name}");
                Console.WriteLine();

                foreach (ProvisioningAction action in provisioningPlan.Actions)
                {
                    WriteProvisioningAction(action);
                }

                Console.WriteLine();
                Console.WriteLine(
                    $"Actions planned: {provisioningPlan.Actions.Count}");

                Console.WriteLine();
                Console.WriteLine(
                    new string('-', 60));

                Console.WriteLine();
            }

            Console.WriteLine(
                "Dry-run provisioning plan completed successfully.");

            Console.WriteLine();

            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine();
            Console.WriteLine("Bootstrap failed.");
            Console.WriteLine();
            Console.WriteLine(ex.Message);
            Console.WriteLine();

            Console.ResetColor();

            return 1;
        }
    }

    private static void WriteProvisioningAction(
        ProvisioningAction action)
    {
        Console.WriteLine(
            $"  [{action.ActionType.ToString().ToUpperInvariant()}] {action.TargetPath}");

        Console.WriteLine(
            $"    Current: {action.CurrentState}");

        Console.WriteLine(
            $"    Desired: {action.DesiredState}");

        Console.WriteLine(
            $"    Reason: {action.Reason}");

        foreach (string warning in action.Warnings)
        {
            Console.WriteLine(
                $"    Warning: {warning}");
        }
    }

    private static int WriteValidationFailure(
        string heading,
        Models.ValidationResult result)
    {
        Console.ForegroundColor =
            ConsoleColor.Red;

        Console.WriteLine();
        Console.WriteLine(heading);
        Console.WriteLine();

        foreach (var error in result.Errors)
        {
            string location =
                string.IsNullOrWhiteSpace(error.PropertyName)
                    ? string.Empty
                    : $" [{error.PropertyName}]";

            Console.WriteLine(
                $"  {error.Code}{location}: {error.Message}");
        }

        Console.WriteLine();
        Console.ResetColor();

        return 1;
    }
}
