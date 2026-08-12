namespace CareerOS.Bootstrap.Models;

public sealed class ValidationResult
{
    private readonly List<ValidationError> _errors = [];
    private readonly List<ValidationWarning> _warnings = [];

    public IReadOnlyList<ValidationError> Errors => _errors;

    public IReadOnlyList<ValidationWarning> Warnings => _warnings;

    public bool IsValid => _errors.Count == 0;

    public void AddError(
        string code,
        string message,
        string? propertyName = null)
    {
        _errors.Add(new ValidationError(
            code,
            message,
            propertyName));
    }

    public void AddWarning(
        string code,
        string message,
        string? propertyName = null)
    {
        _warnings.Add(new ValidationWarning(
            code,
            message,
            propertyName));
    }
}
