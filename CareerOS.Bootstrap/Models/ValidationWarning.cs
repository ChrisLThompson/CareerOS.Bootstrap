namespace CareerOS.Bootstrap.Models;

public sealed record ValidationWarning(
    string Code,
    string Message,
    string? PropertyName = null);
