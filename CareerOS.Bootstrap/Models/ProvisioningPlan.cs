namespace CareerOS.Bootstrap.Models;

public sealed class ProvisioningPlan
{
    public IReadOnlyList<ProvisioningAction> Actions { get; init; } =
        Array.Empty<ProvisioningAction>();
}
