namespace CareerOS.Bootstrap.Models;

public sealed class ProvisioningAction
{
    public string TargetPath { get; init; } = string.Empty;

    public ProvisioningDesiredState DesiredState { get; init; } =
        ProvisioningDesiredState.Directory;

    public ProvisioningCurrentState CurrentState { get; init; }

    public ProvisioningActionType ActionType { get; init; }

    public string Reason { get; init; } = string.Empty;

    public IReadOnlyList<string> Warnings { get; init; } =
        Array.Empty<string>();
}
