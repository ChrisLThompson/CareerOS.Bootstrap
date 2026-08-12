namespace CareerOS.Bootstrap.Models;

public sealed record ValidationError(
    string Code,
    string Message,
    string? PropertyName = null);
