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

            BootstrapConfiguration bootstrapConfiguration =
                jsonService.LoadBootstrapConfiguration(
                    bootstrapPath);

            TemplateConfiguration templateConfiguration =
                jsonService.LoadTemplateConfiguration(
                    templatesPath);

            ValidationResult configurationValidation =
                validationService.Validate(
                    bootstrapConfiguration,
                    templateConfiguration);

            if (!configurationValidation.IsValid)
            {
                return WriteValidationFailure(
                    "Configuration validation failed.",
                    configurationValidation);
            }

            string destinationRoot =
                bootstrapConfiguration.DestinationRoot;

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

            Console.WriteLine("Destination:");
            Console.WriteLine($"  {destinationRoot}");
            Console.WriteLine();

            Console.WriteLine("DRY RUN");
            Console.WriteLine("No directories will be created.");
            Console.WriteLine();

            foreach (ProfileConfiguration profile in bootstrapConfiguration.Profiles)
            {
                CareerTemplate template =
                    templateResolver.ResolveTemplate(
                        templateConfiguration,
                        profile.Template);

                IReadOnlyList<string> directoryPlan =
                    directoryPlanService.BuildPlan(
                        destinationRoot,
                        profile,
                        template);

                ValidationResult planValidation =
                    validationService.ValidatePlannedPaths(
                        destinationRoot,
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
        ValidationResult result)
    {
        Console.ForegroundColor =
            ConsoleColor.Red;

        Console.WriteLine();
        Console.WriteLine(heading);
        Console.WriteLine();

        foreach (ValidationError error in result.Errors)
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
