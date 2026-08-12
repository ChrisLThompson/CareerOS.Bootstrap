using CareerOS.Bootstrap.Models;

namespace CareerOS.Bootstrap.Tests.Models;

public class ValidationResultTests
{
    [Fact]
    public void NewResult_IsValid()
    {
        ValidationResult result = new();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void AddWarning_DoesNotInvalidateResult()
    {
        ValidationResult result = new();

        result.AddWarning(
            "TEST_WARNING",
            "This is a warning.",
            "Example.Property");

        Assert.True(result.IsValid);

        ValidationWarning warning = Assert.Single(result.Warnings);

        Assert.Equal("TEST_WARNING", warning.Code);
        Assert.Equal("This is a warning.", warning.Message);
        Assert.Equal("Example.Property", warning.PropertyName);
    }

    [Fact]
    public void AddError_InvalidatesResult()
    {
        ValidationResult result = new();

        result.AddError(
            "TEST_ERROR",
            "This is an error.",
            "Example.Property");

        Assert.False(result.IsValid);

        ValidationError error = Assert.Single(result.Errors);

        Assert.Equal("TEST_ERROR", error.Code);
        Assert.Equal("This is an error.", error.Message);
        Assert.Equal("Example.Property", error.PropertyName);
    }

    [Fact]
    public void AddMultipleErrors_PreservesAllErrors()
    {
        ValidationResult result = new();

        result.AddError("ERROR_ONE", "First error.");
        result.AddError("ERROR_TWO", "Second error.");

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);

        Assert.Collection(
            result.Errors,
            first =>
            {
                Assert.Equal("ERROR_ONE", first.Code);
                Assert.Equal("First error.", first.Message);
            },
            second =>
            {
                Assert.Equal("ERROR_TWO", second.Code);
                Assert.Equal("Second error.", second.Message);
            });
    }

    [Fact]
    public void AddMultipleWarnings_PreservesAllWarningsAndRemainsValid()
    {
        ValidationResult result = new();

        result.AddWarning("WARNING_ONE", "First warning.");
        result.AddWarning("WARNING_TWO", "Second warning.");

        Assert.True(result.IsValid);
        Assert.Equal(2, result.Warnings.Count);

        Assert.Collection(
            result.Warnings,
            first =>
            {
                Assert.Equal("WARNING_ONE", first.Code);
                Assert.Equal("First warning.", first.Message);
            },
            second =>
            {
                Assert.Equal("WARNING_TWO", second.Code);
                Assert.Equal("Second warning.", second.Message);
            });
    }

    [Fact]
    public void AddErrorAndWarning_ResultIsInvalidBecauseOfError()
    {
        ValidationResult result = new();

        result.AddWarning("TEST_WARNING", "This is a warning.");
        result.AddError("TEST_ERROR", "This is an error.");

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Single(result.Warnings);
    }

    [Fact]
    public void AddErrorAndWarning_WithoutPropertyName_PreservesNullLocation()
    {
        ValidationResult result = new();

        result.AddError("TEST_ERROR", "This is an error.");
        result.AddWarning("TEST_WARNING", "This is a warning.");

        ValidationError error = Assert.Single(result.Errors);
        ValidationWarning warning = Assert.Single(result.Warnings);

        Assert.Null(error.PropertyName);
        Assert.Null(warning.PropertyName);
    }
}
