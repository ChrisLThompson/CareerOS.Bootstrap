using CareerOS.Bootstrap.Models;

namespace CareerOS.Bootstrap.Services;

public sealed class ProvisioningPlanService
{
    public ProvisioningPlan BuildPlan(
        IEnumerable<string> plannedPaths)
    {
        ArgumentNullException.ThrowIfNull(plannedPaths);

        List<ProvisioningAction> actions = [];

        foreach (string? plannedPath in plannedPaths)
        {
            actions.Add(
                ClassifyPath(plannedPath));
        }

        return new ProvisioningPlan
        {
            Actions = actions
        };
    }

    private static ProvisioningAction ClassifyPath(
        string? plannedPath)
    {
        if (string.IsNullOrWhiteSpace(plannedPath))
        {
            return CreateRejectedAction(
                plannedPath ?? string.Empty,
                ProvisioningCurrentState.Invalid,
                "Planned path is required.");
        }

        if (!Path.IsPathFullyQualified(plannedPath))
        {
            return CreateRejectedAction(
                plannedPath,
                ProvisioningCurrentState.Invalid,
                "Planned path must be fully qualified.");
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
            return CreateRejectedAction(
                plannedPath,
                ProvisioningCurrentState.Invalid,
                "Planned path is invalid.");
        }

        if (Directory.Exists(normalizedPath))
        {
            return new ProvisioningAction
            {
                TargetPath = normalizedPath,
                DesiredState =
                    ProvisioningDesiredState.Directory,
                CurrentState =
                    ProvisioningCurrentState.Directory,
                ActionType =
                    ProvisioningActionType.Preserve,
                Reason =
                    "Expected directory already exists.",
                Warnings =
                [
                    "Existing directory will not be modified."
                ]
            };
        }

        if (File.Exists(normalizedPath))
        {
            return new ProvisioningAction
            {
                TargetPath = normalizedPath,
                DesiredState =
                    ProvisioningDesiredState.Directory,
                CurrentState =
                    ProvisioningCurrentState.File,
                ActionType =
                    ProvisioningActionType.Conflict,
                Reason =
                    "A file exists where a directory is required."
            };
        }

        return new ProvisioningAction
        {
            TargetPath = normalizedPath,
            DesiredState =
                ProvisioningDesiredState.Directory,
            CurrentState =
                ProvisioningCurrentState.Missing,
            ActionType =
                ProvisioningActionType.Create,
            Reason =
                "Expected directory is missing."
        };
    }

    private static ProvisioningAction CreateRejectedAction(
        string targetPath,
        ProvisioningCurrentState currentState,
        string reason)
    {
        return new ProvisioningAction
        {
            TargetPath = targetPath,
            DesiredState =
                ProvisioningDesiredState.Directory,
            CurrentState = currentState,
            ActionType =
                ProvisioningActionType.Reject,
            Reason = reason
        };
    }
}
