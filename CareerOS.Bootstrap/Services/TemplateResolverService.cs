using CareerOS.Bootstrap.Models;

namespace CareerOS.Bootstrap.Services;

public class TemplateResolverService
{
    public CareerTemplate ResolveTemplate(
        TemplateConfiguration configuration,
        string templateName)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.IsNullOrWhiteSpace(templateName))
        {
            throw new ArgumentException(
                "Template name cannot be empty.",
                nameof(templateName));
        }

        CareerTemplate? template =
            configuration.Templates.FirstOrDefault(
                template => string.Equals(
                    template.Name,
                    templateName,
                    StringComparison.OrdinalIgnoreCase));

        return template
            ?? throw new InvalidOperationException(
                $"Template '{templateName}' was not found in the template configuration.");
    }
}