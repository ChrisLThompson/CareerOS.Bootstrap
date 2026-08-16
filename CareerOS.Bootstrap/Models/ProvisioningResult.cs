using CareerOS.Bootstrap.Models;

namespace CareerOS.Bootstrap.Models;

public sealed class ProvisioningResult
{
    public ValidationResult Validation { get; init; } =
        new();

    public int CreatedCount { get; init; }

    public int PreservedCount { get; init; }

    public bool Succeeded { get; init; }
}
