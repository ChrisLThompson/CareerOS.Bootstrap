using System.Text.Json;
using CareerOS.Bootstrap.Models;
using CareerOS.Bootstrap.Services;

namespace CareerOS.Bootstrap.Tests.Services;

public class JsonConfigurationServiceTests : IDisposable
{
    private readonly JsonConfigurationService _service = new();
    private readonly string _tempDirectory;

    public JsonConfigurationServiceTests()
    {
        _tempDirectory = Path.Combine(
            Path.GetTempPath(),
            "CareerOS.Bootstrap.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void LoadBootstrapConfiguration_WithValidJson_ReturnsConfiguration()
    {
        string path = WriteFile(
            "bootstrap.json",
            """
            {
              "profiles": [
                {
                  "name": "Chris",
                  "directory": "CareerOS_Chris",
                  "template": "CareerProfessional"
                }
              ]
            }
            """);

        BootstrapConfiguration result =
            _service.LoadBootstrapConfiguration(path);

        ProfileConfiguration profile = Assert.Single(result.Profiles);

        Assert.Equal("Chris", profile.Name);
        Assert.Equal("CareerOS_Chris", profile.Directory);
        Assert.Equal("CareerProfessional", profile.Template);
    }

    [Fact]
    public void LoadTemplateConfiguration_WithValidJson_ReturnsRecursiveTemplate()
    {
        string path = WriteFile(
            "templates.json",
            """
            {
              "templates": [
                {
                  "name": "CareerProfessional",
                  "directories": [
                    {
                      "name": "Resume",
                      "children": [
                        {
                          "name": "Master",
                          "children": []
                        }
                      ]
                    }
                  ]
                }
              ]
            }
            """);

        TemplateConfiguration result =
            _service.LoadTemplateConfiguration(path);

        CareerTemplate template = Assert.Single(result.Templates);
        DirectoryNode root = Assert.Single(template.Directories);
        DirectoryNode child = Assert.Single(root.Children);

        Assert.Equal("CareerProfessional", template.Name);
        Assert.Equal("Resume", root.Name);
        Assert.Equal("Master", child.Name);
    }

    [Fact]
    public void LoadBootstrapConfiguration_WithDifferentPropertyCasing_DeserializesSuccessfully()
    {
        string path = WriteFile(
            "bootstrap.json",
            """
            {
              "PROFILES": [
                {
                  "NAME": "Chris",
                  "DIRECTORY": "CareerOS_Chris",
                  "TEMPLATE": "CareerProfessional"
                }
              ]
            }
            """);

        BootstrapConfiguration result =
            _service.LoadBootstrapConfiguration(path);

        ProfileConfiguration profile = Assert.Single(result.Profiles);

        Assert.Equal("Chris", profile.Name);
        Assert.Equal("CareerOS_Chris", profile.Directory);
        Assert.Equal("CareerProfessional", profile.Template);
    }

    [Fact]
    public void LoadTemplateConfiguration_WithComments_DeserializesSuccessfully()
    {
        string path = WriteFile(
            "templates.json",
            """
            {
              // This comment is intentionally supported.
              "templates": [
                {
                  "name": "CareerProfessional",
                  "directories": []
                }
              ]
            }
            """);

        TemplateConfiguration result =
            _service.LoadTemplateConfiguration(path);

        CareerTemplate template = Assert.Single(result.Templates);

        Assert.Equal("CareerProfessional", template.Name);
    }

    [Fact]
    public void LoadTemplateConfiguration_WithTrailingCommas_DeserializesSuccessfully()
    {
        string path = WriteFile(
            "templates.json",
            """
            {
              "templates": [
                {
                  "name": "CareerProfessional",
                  "directories": [],
                },
              ],
            }
            """);

        TemplateConfiguration result =
            _service.LoadTemplateConfiguration(path);

        CareerTemplate template = Assert.Single(result.Templates);

        Assert.Equal("CareerProfessional", template.Name);
    }

    [Fact]
    public void LoadBootstrapConfiguration_WithMissingFile_ThrowsFileNotFoundException()
    {
        string path = Path.Combine(
            _tempDirectory,
            "missing-bootstrap.json");

        FileNotFoundException exception =
            Assert.Throws<FileNotFoundException>(
                () => _service.LoadBootstrapConfiguration(path));

        Assert.Equal(path, exception.FileName);
        Assert.Contains(
            $"Configuration file was not found: {path}",
            exception.Message);
    }

    [Fact]
    public void LoadTemplateConfiguration_WithMissingFile_ThrowsFileNotFoundException()
    {
        string path = Path.Combine(
            _tempDirectory,
            "missing-templates.json");

        FileNotFoundException exception =
            Assert.Throws<FileNotFoundException>(
                () => _service.LoadTemplateConfiguration(path));

        Assert.Equal(path, exception.FileName);
        Assert.Contains(
            $"Configuration file was not found: {path}",
            exception.Message);
    }

    [Fact]
    public void LoadBootstrapConfiguration_WithMalformedJson_ThrowsJsonException()
    {
        string path = WriteFile(
            "bootstrap.json",
            """
            {
              "profiles": [
            }
            """);

        Assert.Throws<JsonException>(
            () => _service.LoadBootstrapConfiguration(path));
    }

    [Fact]
    public void LoadTemplateConfiguration_WithMalformedJson_ThrowsJsonException()
    {
        string path = WriteFile(
            "templates.json",
            """
            {
              "templates":
            }
            """);

        Assert.Throws<JsonException>(
            () => _service.LoadTemplateConfiguration(path));
    }

    [Fact]
    public void LoadBootstrapConfiguration_WithJsonNull_ThrowsInvalidOperationException()
    {
        string path = WriteFile(
            "bootstrap.json",
            "null");

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => _service.LoadBootstrapConfiguration(path));

        Assert.Equal(
            $"Unable to deserialize bootstrap configuration: {path}",
            exception.Message);
    }

    [Fact]
    public void LoadTemplateConfiguration_WithJsonNull_ThrowsInvalidOperationException()
    {
        string path = WriteFile(
            "templates.json",
            "null");

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => _service.LoadTemplateConfiguration(path));

        Assert.Equal(
            $"Unable to deserialize template configuration: {path}",
            exception.Message);
    }

    [Fact]
    public void LoadBootstrapConfiguration_WithEmptyProfiles_ReturnsEmptyCollection()
    {
        string path = WriteFile(
            "bootstrap.json",
            """
            {
              "profiles": []
            }
            """);

        BootstrapConfiguration result =
            _service.LoadBootstrapConfiguration(path);

        Assert.Empty(result.Profiles);
    }

    [Fact]
    public void LoadTemplateConfiguration_WithEmptyTemplates_ReturnsEmptyCollection()
    {
        string path = WriteFile(
            "templates.json",
            """
            {
              "templates": []
            }
            """);

        TemplateConfiguration result =
            _service.LoadTemplateConfiguration(path);

        Assert.Empty(result.Templates);
    }

    private string WriteFile(
        string fileName,
        string contents)
    {
        string path = Path.Combine(
            _tempDirectory,
            fileName);

        File.WriteAllText(path, contents);

        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(
                _tempDirectory,
                recursive: true);
        }

        GC.SuppressFinalize(this);
    }
}
