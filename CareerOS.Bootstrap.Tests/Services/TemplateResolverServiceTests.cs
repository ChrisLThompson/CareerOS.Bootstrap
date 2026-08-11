using CareerOS.Bootstrap.Models;
using CareerOS.Bootstrap.Services;

namespace CareerOS.Bootstrap.Tests.Services;

public class TemplateResolverServiceTests
{
    private readonly TemplateResolverService _service = new();

    [Fact]
    public void ResolveTemplate_WithExactName_ReturnsMatchingTemplate()
    {
        CareerTemplate expected = CreateTemplate("CareerProfessional");

        TemplateConfiguration configuration = new()
        {
            Templates =
            [
                expected,
                CreateTemplate("HealthcareProfessional")
            ]
        };

        CareerTemplate result =
            _service.ResolveTemplate(configuration, "CareerProfessional");

        Assert.Same(expected, result);
    }

    [Fact]
    public void ResolveTemplate_WithDifferentCasing_ReturnsMatchingTemplate()
    {
        CareerTemplate expected = CreateTemplate("CareerProfessional");

        TemplateConfiguration configuration = new()
        {
            Templates =
            [
                expected
            ]
        };

        CareerTemplate result =
            _service.ResolveTemplate(configuration, "careerprofessional");

        Assert.Same(expected, result);
    }

    [Fact]
    public void ResolveTemplate_WithMultipleTemplates_ReturnsOnlyRequestedTemplate()
    {
        CareerTemplate careerProfessional =
            CreateTemplate("CareerProfessional");

        CareerTemplate healthcareProfessional =
            CreateTemplate("HealthcareProfessional");

        TemplateConfiguration configuration = new()
        {
            Templates =
            [
                careerProfessional,
                healthcareProfessional
            ]
        };

        CareerTemplate result =
            _service.ResolveTemplate(
                configuration,
                "HealthcareProfessional");

        Assert.Same(healthcareProfessional, result);
        Assert.NotSame(careerProfessional, result);
    }

    [Fact]
    public void ResolveTemplate_WithNullConfiguration_ThrowsArgumentNullException()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => _service.ResolveTemplate(
                    null!,
                    "CareerProfessional"));

        Assert.Equal("configuration", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void ResolveTemplate_WithMissingTemplateName_ThrowsArgumentException(
        string? templateName)
    {
        TemplateConfiguration configuration = new();

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => _service.ResolveTemplate(
                    configuration,
                    templateName!));

        Assert.Equal("templateName", exception.ParamName);
        Assert.Contains(
            "Template name cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void ResolveTemplate_WithUnknownTemplate_ThrowsInvalidOperationException()
    {
        TemplateConfiguration configuration = new()
        {
            Templates =
            [
                CreateTemplate("CareerProfessional")
            ]
        };

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => _service.ResolveTemplate(
                    configuration,
                    "UnknownTemplate"));

        Assert.Equal(
            "Template 'UnknownTemplate' was not found in the template configuration.",
            exception.Message);
    }

    [Fact]
    public void ResolveTemplate_WithEmptyTemplateCollection_ThrowsInvalidOperationException()
    {
        TemplateConfiguration configuration = new();

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => _service.ResolveTemplate(
                    configuration,
                    "CareerProfessional"));

        Assert.Equal(
            "Template 'CareerProfessional' was not found in the template configuration.",
            exception.Message);
    }

    private static CareerTemplate CreateTemplate(string name)
    {
        return new CareerTemplate
        {
            Name = name
        };
    }
}
