using CareerOS.Bootstrap.Models;
using CareerOS.Bootstrap.Services;

namespace CareerOS.Bootstrap.Tests.Services;

public class DirectoryPlanServiceTests
{
    private readonly DirectoryPlanService _service = new();

    [Fact]
    public void BuildPlan_WithProfileAndNoTemplateDirectories_ReturnsProfileRootOnly()
    {
        ProfileConfiguration profile = CreateProfile("Chris", "CareerOS_Chris");
        CareerTemplate template = CreateTemplate();

        IReadOnlyList<string> result =
            _service.BuildPlan(
                "D:\\CareerOS",
                profile,
                template);

        string expectedRoot =
            Path.Combine(
                "D:\\CareerOS",
                "CareerOS_Chris");

        string actual = Assert.Single(result);

        Assert.Equal(expectedRoot, actual);
    }

    [Fact]
    public void BuildPlan_WithTopLevelDirectories_ReturnsProfileRootAndDirectories()
    {
        ProfileConfiguration profile = CreateProfile("Chris", "CareerOS_Chris");
        CareerTemplate template = CreateTemplate(
            new DirectoryNode { Name = "Resume" },
            new DirectoryNode { Name = "Applications" });

        IReadOnlyList<string> result =
            _service.BuildPlan(
                "D:\\CareerOS",
                profile,
                template);

        string profileRoot =
            Path.Combine(
                "D:\\CareerOS",
                "CareerOS_Chris");

        Assert.Equal(
            [
                profileRoot,
                Path.Combine(profileRoot, "Resume"),
                Path.Combine(profileRoot, "Applications")
            ],
            result);
    }

    [Fact]
    public void BuildPlan_WithNestedDirectories_ReturnsRecursivePathsInTraversalOrder()
    {
        ProfileConfiguration profile = CreateProfile("Chris", "CareerOS_Chris");

        CareerTemplate template = CreateTemplate(
            new DirectoryNode
            {
                Name = "Resume",
                Children =
                [
                    new DirectoryNode
                    {
                        Name = "Master",
                        Children =
                        [
                            new DirectoryNode
                            {
                                Name = "Archived"
                            }
                        ]
                    },
                    new DirectoryNode
                    {
                        Name = "RC"
                    }
                ]
            });

        IReadOnlyList<string> result =
            _service.BuildPlan(
                "D:\\CareerOS",
                profile,
                template);

        string profileRoot =
            Path.Combine(
                "D:\\CareerOS",
                "CareerOS_Chris");

        Assert.Equal(
            [
                profileRoot,
                Path.Combine(profileRoot, "Resume"),
                Path.Combine(profileRoot, "Resume", "Master"),
                Path.Combine(profileRoot, "Resume", "Master", "Archived"),
                Path.Combine(profileRoot, "Resume", "RC")
            ],
            result);
    }

    [Fact]
    public void BuildPlan_WithMultipleBranches_ReturnsAllRecursivePaths()
    {
        ProfileConfiguration profile = CreateProfile("Chris", "CareerOS_Chris");

        CareerTemplate template = CreateTemplate(
            new DirectoryNode
            {
                Name = "Resume",
                Children =
                [
                    new DirectoryNode { Name = "Master" },
                    new DirectoryNode { Name = "Archived" }
                ]
            },
            new DirectoryNode
            {
                Name = "Applications",
                Children =
                [
                    new DirectoryNode
                    {
                        Name = "Active",
                        Children =
                        [
                            new DirectoryNode { Name = "CompanyA" }
                        ]
                    }
                ]
            });

        IReadOnlyList<string> result =
            _service.BuildPlan(
                "D:\\CareerOS",
                profile,
                template);

        string profileRoot =
            Path.Combine(
                "D:\\CareerOS",
                "CareerOS_Chris");

        Assert.Equal(
            [
                profileRoot,
                Path.Combine(profileRoot, "Resume"),
                Path.Combine(profileRoot, "Resume", "Master"),
                Path.Combine(profileRoot, "Resume", "Archived"),
                Path.Combine(profileRoot, "Applications"),
                Path.Combine(profileRoot, "Applications", "Active"),
                Path.Combine(profileRoot, "Applications", "Active", "CompanyA")
            ],
            result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void BuildPlan_WithMissingBasePath_ThrowsArgumentException(
        string? basePath)
    {
        ProfileConfiguration profile = CreateProfile("Chris", "CareerOS_Chris");
        CareerTemplate template = CreateTemplate();

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => _service.BuildPlan(
                    basePath!,
                    profile,
                    template));

        Assert.Equal("basePath", exception.ParamName);
        Assert.Contains(
            "Base path cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void BuildPlan_WithNullProfile_ThrowsArgumentNullException()
    {
        CareerTemplate template = CreateTemplate();

        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => _service.BuildPlan(
                    "D:\\CareerOS",
                    null!,
                    template));

        Assert.Equal("profile", exception.ParamName);
    }

    [Fact]
    public void BuildPlan_WithNullTemplate_ThrowsArgumentNullException()
    {
        ProfileConfiguration profile = CreateProfile("Chris", "CareerOS_Chris");

        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => _service.BuildPlan(
                    "D:\\CareerOS",
                    profile,
                    null!));

        Assert.Equal("template", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void BuildPlan_WithMissingProfileDirectory_ThrowsInvalidOperationException(
        string directory)
    {
        ProfileConfiguration profile = CreateProfile("Chris", directory);
        CareerTemplate template = CreateTemplate();

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => _service.BuildPlan(
                    "D:\\CareerOS",
                    profile,
                    template));

        Assert.Equal(
            "Profile 'Chris' does not define a directory.",
            exception.Message);
    }

    [Fact]
    public void BuildPlan_WithUnnamedTopLevelDirectory_ThrowsInvalidOperationException()
    {
        ProfileConfiguration profile = CreateProfile("Chris", "CareerOS_Chris");

        CareerTemplate template = CreateTemplate(
            new DirectoryNode
            {
                Name = string.Empty
            });

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => _service.BuildPlan(
                    "D:\\CareerOS",
                    profile,
                    template));

        Assert.Equal(
            "A directory template contains a directory with no name.",
            exception.Message);
    }

    [Fact]
    public void BuildPlan_WithUnnamedNestedDirectory_ThrowsInvalidOperationException()
    {
        ProfileConfiguration profile = CreateProfile("Chris", "CareerOS_Chris");

        CareerTemplate template = CreateTemplate(
            new DirectoryNode
            {
                Name = "Resume",
                Children =
                [
                    new DirectoryNode
                    {
                        Name = "   "
                    }
                ]
            });

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => _service.BuildPlan(
                    "D:\\CareerOS",
                    profile,
                    template));

        Assert.Equal(
            "A directory template contains a directory with no name.",
            exception.Message);
    }

    [Fact]
    public void BuildPlan_DoesNotCreateDirectoriesOnFilesystem()
    {
        string basePath =
            Path.Combine(
                Path.GetTempPath(),
                "CareerOS.Bootstrap.Tests",
                Guid.NewGuid().ToString("N"));

        ProfileConfiguration profile = CreateProfile("Chris", "CareerOS_Chris");

        CareerTemplate template = CreateTemplate(
            new DirectoryNode
            {
                Name = "Resume"
            });

        IReadOnlyList<string> result =
            _service.BuildPlan(
                basePath,
                profile,
                template);

        Assert.NotEmpty(result);
        Assert.False(Directory.Exists(basePath));
        Assert.False(
            Directory.Exists(
                Path.Combine(
                    basePath,
                    profile.Directory)));
    }

    private static ProfileConfiguration CreateProfile(
        string name,
        string directory)
    {
        return new ProfileConfiguration
        {
            Name = name,
            Directory = directory,
            Template = "CareerProfessional"
        };
    }

    private static CareerTemplate CreateTemplate(
        params DirectoryNode[] directories)
    {
        return new CareerTemplate
        {
            Name = "CareerProfessional",
            Directories = [.. directories]
        };
    }
}
