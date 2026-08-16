using CareerOS.Bootstrap.Models;

namespace CareerOS.Bootstrap.Services;

public sealed class ProvisioningService
{
    private readonly ConfigurationValidationService _validationService;

    public ProvisioningService(
        ConfigurationValidationService validationService)
    {
        ArgumentNullException.ThrowIfNull(validationService);

        _validationService = validationService;
    }

    public ProvisioningResult Provision(
        string destinationRoot,
        ProvisioningPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        IReadOnlyList<ProvisioningAction> actions =
            plan.Actions;

        ValidationResult validation =
            _validationService.ValidatePlannedPaths(
                destinationRoot,
                actions.Select(action => action.TargetPath));

        if (!validation.IsValid)
        {
            return Failure(validation);
        }

        ValidateActionIntent(
            actions,
            validation);

        if (!validation.IsValid)
        {
            return Failure(validation);
        }

        List<ExecutionDecision> decisions =
            PreflightActions(
                actions,
                validation);

        if (!validation.IsValid)
        {
            return Failure(validation);
        }

        int createdCount = 0;
        int preservedCount = 0;

        foreach (ExecutionDecision decision in decisions)
        {
            switch (decision.ActionType)
            {
                case ProvisioningActionType.Create:
                    try
                    {
                        Directory.CreateDirectory(
                            decision.TargetPath);

                        createdCount++;
                    }
                    catch (Exception exception)
                        when (exception is IOException or
                              UnauthorizedAccessException or
                              ArgumentException or
                              NotSupportedException or
                              PathTooLongException)
                    {
                        validation.AddError(
                            "PROVISIONING_CREATE_FAILED",
                            $"Failed to create directory '{decision.TargetPath}': {exception.Message}",
                            decision.TargetPath);

                        return new ProvisioningResult
                        {
                            Validation = validation,
                            CreatedCount = createdCount,
                            PreservedCount = preservedCount,
                            Succeeded = false
                        };
                    }

                    break;

                case ProvisioningActionType.Preserve:
                    preservedCount++;
                    break;

                case ProvisioningActionType.Skip:
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unexpected execution decision '{decision.ActionType}'.");
            }
        }

        return new ProvisioningResult
        {
            Validation = validation,
            CreatedCount = createdCount,
            PreservedCount = preservedCount,
            Succeeded = true
        };
    }

    private static void ValidateActionIntent(
        IReadOnlyList<ProvisioningAction> actions,
        ValidationResult validation)
    {
        for (int index = 0; index < actions.Count; index++)
        {
            ProvisioningAction action =
                actions[index];

            string propertyName =
                $"Actions[{index}]";

            switch (action.ActionType)
            {
                case ProvisioningActionType.Conflict:
                    validation.AddError(
                        "PROVISIONING_ACTION_CONFLICT",
                        $"Provisioning cannot continue because '{action.TargetPath}' is classified as a conflict.",
                        propertyName);
                    break;

                case ProvisioningActionType.Reject:
                    validation.AddError(
                        "PROVISIONING_ACTION_REJECTED",
                        $"Provisioning cannot continue because '{action.TargetPath}' is classified as rejected.",
                        propertyName);
                    break;
            }
        }
    }

    private static List<ExecutionDecision> PreflightActions(
        IReadOnlyList<ProvisioningAction> actions,
        ValidationResult validation)
    {
        List<ExecutionDecision> decisions = [];

        for (int index = 0; index < actions.Count; index++)
        {
            ProvisioningAction action =
                actions[index];

            string propertyName =
                $"Actions[{index}]";

            if (action.ActionType ==
                ProvisioningActionType.Skip)
            {
                decisions.Add(
                    new ExecutionDecision(
                        action.TargetPath,
                        ProvisioningActionType.Skip));

                continue;
            }

            bool directoryExists =
                Directory.Exists(action.TargetPath);

            bool fileExists =
                File.Exists(action.TargetPath);

            if (fileExists)
            {
                validation.AddError(
                    "PROVISIONING_WRITE_TIME_CONFLICT",
                    $"A file exists where directory '{action.TargetPath}' is required.",
                    propertyName);

                continue;
            }

            if (action.ActionType ==
                ProvisioningActionType.Create)
            {
                decisions.Add(
                    new ExecutionDecision(
                        action.TargetPath,
                        directoryExists
                            ? ProvisioningActionType.Preserve
                            : ProvisioningActionType.Create));

                continue;
            }

            if (action.ActionType ==
                ProvisioningActionType.Preserve)
            {
                if (!directoryExists)
                {
                    validation.AddError(
                        "PROVISIONING_STALE_PRESERVE_ACTION",
                        $"Directory '{action.TargetPath}' no longer exists. Rebuild the provisioning plan before writing.",
                        propertyName);

                    continue;
                }

                decisions.Add(
                    new ExecutionDecision(
                        action.TargetPath,
                        ProvisioningActionType.Preserve));

                continue;
            }

            validation.AddError(
                "PROVISIONING_ACTION_UNSUPPORTED",
                $"Provisioning action '{action.ActionType}' is not supported for execution.",
                propertyName);
        }

        return decisions;
    }

    private static ProvisioningResult Failure(
        ValidationResult validation)
    {
        return new ProvisioningResult
        {
            Validation = validation,
            Succeeded = false
        };
    }

    private sealed record ExecutionDecision(
        string TargetPath,
        ProvisioningActionType ActionType);
}
