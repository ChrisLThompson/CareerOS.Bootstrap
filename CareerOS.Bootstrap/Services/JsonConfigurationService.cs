using System.Text.Json;
using CareerOS.Bootstrap.Models;

namespace CareerOS.Bootstrap.Services;

public class JsonConfigurationService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public BootstrapConfiguration LoadBootstrapConfiguration(string path)
    {
        ValidateFile(path);

        string json = File.ReadAllText(path);

        BootstrapConfiguration? configuration =
            JsonSerializer.Deserialize<BootstrapConfiguration>(
                json,
                _jsonOptions);

        return configuration
            ?? throw new InvalidOperationException(
                $"Unable to deserialize bootstrap configuration: {path}");
    }

    public TemplateConfiguration LoadTemplateConfiguration(string path)
    {
        ValidateFile(path);

        string json = File.ReadAllText(path);

        TemplateConfiguration? configuration =
            JsonSerializer.Deserialize<TemplateConfiguration>(
                json,
                _jsonOptions);

        return configuration
            ?? throw new InvalidOperationException(
                $"Unable to deserialize template configuration: {path}");
    }

    private static void ValidateFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Configuration file was not found: {path}",
                path);
        }
    }
}