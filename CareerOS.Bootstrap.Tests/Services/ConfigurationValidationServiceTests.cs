using CareerOS.Bootstrap.Models;
using CareerOS.Bootstrap.Services;

namespace CareerOS.Bootstrap.Tests.Services;

public class ConfigurationValidationServiceTests
{
    private readonly ConfigurationValidationService _service = new();

    [Fact]
    public void Validate_WithValidConfiguration_ReturnsValidResult()
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    "Chris",
                    "CareerOS_Chris",
                    "CareerProfessional"));

        TemplateConfiguration templates =
            CreateTemplates(
                CreateTemplate(
                    "CareerProfessional",
                    new DirectoryNode
                    {
                        Name = "Resume",
                        Children =
                        [
                            new DirectoryNode
                            {
                                Name = "Master"
                            }
                        ]
                    }));

        ValidationResult result =
            _service.Validate(
                bootstrap,
                templates);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Validate_WithEmptyProfiles_ReturnsProfileCollectionError()
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap();

        TemplateConfiguration templates =
            CreateTemplates(
                CreateTemplate("CareerProfessional"));

        ValidationResult result =
            _service.Validate(
                bootstrap,
                templates);

        ValidationError error =
            Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Equal("PROFILE_COLLECTION_EMPTY", error.Code);
        Assert.Equal("Profiles", error.PropertyName);
    }

    [Fact]
    public void Validate_WithEmptyTemplates_ReturnsTemplateCollectionError()
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    "Chris",
                    "CareerOS_Chris",
                    "CareerProfessional"));

        TemplateConfiguration templates =
            CreateTemplates();

        ValidationResult result =
            _service.Validate(
                bootstrap,
                templates);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.Code == "TEMPLATE_COLLECTION_EMPTY" &&
                error.PropertyName == "Templates");

        Assert.Contains(
            result.Errors,
            error =>
                error.Code == "PROFILE_TEMPLATE_NOT_FOUND" &&
                error.PropertyName == "Profiles[0].Template");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Validate_WithMissingProfileName_ReturnsRequiredError(
        string name)
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    name,
                    "CareerOS_Chris",
                    "CareerProfessional"));

        ValidationResult result =
            _service.Validate(
                bootstrap,
                CreateTemplates(
                    CreateTemplate("CareerProfessional")));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "PROFILE_NAME_REQUIRED");

        Assert.Equal(
            "Profiles[0].Name",
            error.PropertyName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Validate_WithMissingProfileDirectory_ReturnsRequiredError(
        string directory)
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    "Chris",
                    directory,
                    "CareerProfessional"));

        ValidationResult result =
            _service.Validate(
                bootstrap,
                CreateTemplates(
                    CreateTemplate("CareerProfessional")));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "PROFILE_DIRECTORY_REQUIRED");

        Assert.Equal(
            "Profiles[0].Directory",
            error.PropertyName);
    }

    [Theory]
    [InlineData("Career:OS")]
    [InlineData("Career/OS")]
    [InlineData("Career\\OS")]
    [InlineData("Career?OS")]
    [InlineData("Career*OS")]
    [InlineData("CareerOS.")]
    [InlineData("CareerOS ")]
    public void Validate_WithInvalidProfileDirectory_ReturnsFilesystemError(
        string directory)
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    "Chris",
                    directory,
                    "CareerProfessional"));

        ValidationResult result =
            _service.Validate(
                bootstrap,
                CreateTemplates(
                    CreateTemplate("CareerProfessional")));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "PROFILE_DIRECTORY_INVALID");

        Assert.Equal(
            "Profiles[0].Directory",
            error.PropertyName);
    }

    [Theory]
    [InlineData("CON")]
    [InlineData("con")]
    [InlineData("PRN")]
    [InlineData("AUX")]
    [InlineData("NUL")]
    [InlineData("COM1")]
    [InlineData("LPT9")]
    [InlineData("CON.txt")]
    public void Validate_WithReservedProfileDirectory_ReturnsReservedError(
        string directory)
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    "Chris",
                    directory,
                    "CareerProfessional"));

        ValidationResult result =
            _service.Validate(
                bootstrap,
                CreateTemplates(
                    CreateTemplate("CareerProfessional")));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "PROFILE_DIRECTORY_RESERVED");

        Assert.Equal(
            "Profiles[0].Directory",
            error.PropertyName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Validate_WithMissingProfileTemplate_ReturnsRequiredError(
        string template)
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    "Chris",
                    "CareerOS_Chris",
                    template));

        ValidationResult result =
            _service.Validate(
                bootstrap,
                CreateTemplates(
                    CreateTemplate("CareerProfessional")));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "PROFILE_TEMPLATE_REQUIRED");

        Assert.Equal(
            "Profiles[0].Template",
            error.PropertyName);

        Assert.DoesNotContain(
            result.Errors,
            error => error.Code == "PROFILE_TEMPLATE_NOT_FOUND");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Validate_WithMissingTemplateName_ReturnsRequiredError(
        string name)
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    "Chris",
                    "CareerOS_Chris",
                    "CareerProfessional"));

        ValidationResult result =
            _service.Validate(
                bootstrap,
                CreateTemplates(
                    CreateTemplate(name)));

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.Code == "TEMPLATE_NAME_REQUIRED" &&
                error.PropertyName == "Templates[0].Name");

        Assert.Contains(
            result.Errors,
            error =>
                error.Code == "PROFILE_TEMPLATE_NOT_FOUND" &&
                error.PropertyName == "Profiles[0].Template");
    }

    [Fact]
    public void Validate_WithDuplicateProfileNames_ReturnsDuplicateError()
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    "Chris",
                    "CareerOS_Chris",
                    "CareerProfessional"),
                CreateProfile(
                    "chris",
                    "CareerOS_Chris_Second",
                    "CareerProfessional"));

        ValidationResult result =
            _service.Validate(
                bootstrap,
                CreateTemplates(
                    CreateTemplate("CareerProfessional")));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "DUPLICATE_PROFILE_NAME");

        Assert.Equal(
            "Profiles[1].Name",
            error.PropertyName);
    }

    [Fact]
    public void Validate_WithDuplicateProfileDirectories_ReturnsDuplicateError()
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    "Chris",
                    "CareerOS_Chris",
                    "CareerProfessional"),
                CreateProfile(
                    "Katie",
                    "careeros_chris",
                    "CareerProfessional"));

        ValidationResult result =
            _service.Validate(
                bootstrap,
                CreateTemplates(
                    CreateTemplate("CareerProfessional")));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "DUPLICATE_PROFILE_DIRECTORY");

        Assert.Equal(
            "Profiles[1].Directory",
            error.PropertyName);
    }

    [Fact]
    public void Validate_WithDuplicateTemplateNames_ReturnsDuplicateError()
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    "Chris",
                    "CareerOS_Chris",
                    "CareerProfessional"));

        TemplateConfiguration templates =
            CreateTemplates(
                CreateTemplate("CareerProfessional"),
                CreateTemplate("careerprofessional"));

        ValidationResult result =
            _service.Validate(
                bootstrap,
                templates);

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "DUPLICATE_TEMPLATE_NAME");

        Assert.Equal(
            "Templates[1].Name",
            error.PropertyName);
    }

    [Fact]
    public void Validate_WithUnknownTemplateReference_ReturnsReferenceError()
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    "Chris",
                    "CareerOS_Chris",
                    "UnknownTemplate"));

        ValidationResult result =
            _service.Validate(
                bootstrap,
                CreateTemplates(
                    CreateTemplate("CareerProfessional")));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "PROFILE_TEMPLATE_NOT_FOUND");

        Assert.Equal(
            "Profiles[0].Template",
            error.PropertyName);

        Assert.Contains(
            "UnknownTemplate",
            error.Message);
    }

    [Fact]
    public void Validate_TemplateReferenceMatching_IsCaseInsensitive()
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    "Chris",
                    "CareerOS_Chris",
                    "careerprofessional"));

        ValidationResult result =
            _service.Validate(
                bootstrap,
                CreateTemplates(
                    CreateTemplate("CareerProfessional")));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Validate_WithUnnamedDirectoryNode_ReturnsRequiredError(
        string name)
    {
        DirectoryNode node =
            new()
            {
                Name = name
            };

        ValidationResult result =
            _service.Validate(
                CreateBootstrap(
                    CreateProfile(
                        "Chris",
                        "CareerOS_Chris",
                        "CareerProfessional")),
                CreateTemplates(
                    CreateTemplate(
                        "CareerProfessional",
                        node)));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "DIRECTORY_NAME_REQUIRED");

        Assert.Equal(
            "Templates[0].Directories[0].Name",
            error.PropertyName);
    }

    [Theory]
    [InlineData("Bad:Name")]
    [InlineData("Bad/Name")]
    [InlineData("Bad\\Name")]
    [InlineData("Bad?Name")]
    [InlineData("Bad*Name")]
    [InlineData("BadName.")]
    [InlineData("BadName ")]
    public void Validate_WithInvalidDirectoryNodeName_ReturnsFilesystemError(
        string name)
    {
        ValidationResult result =
            _service.Validate(
                CreateBootstrap(
                    CreateProfile(
                        "Chris",
                        "CareerOS_Chris",
                        "CareerProfessional")),
                CreateTemplates(
                    CreateTemplate(
                        "CareerProfessional",
                        new DirectoryNode
                        {
                            Name = name
                        })));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "DIRECTORY_NAME_INVALID");

        Assert.Equal(
            "Templates[0].Directories[0].Name",
            error.PropertyName);
    }

    [Theory]
    [InlineData("CON")]
    [InlineData("con")]
    [InlineData("PRN")]
    [InlineData("AUX")]
    [InlineData("NUL")]
    [InlineData("COM9")]
    [InlineData("LPT1")]
    [InlineData("PRN.log")]
    public void Validate_WithReservedDirectoryNodeName_ReturnsReservedError(
        string name)
    {
        ValidationResult result =
            _service.Validate(
                CreateBootstrap(
                    CreateProfile(
                        "Chris",
                        "CareerOS_Chris",
                        "CareerProfessional")),
                CreateTemplates(
                    CreateTemplate(
                        "CareerProfessional",
                        new DirectoryNode
                        {
                            Name = name
                        })));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "DIRECTORY_NAME_RESERVED");

        Assert.Equal(
            "Templates[0].Directories[0].Name",
            error.PropertyName);
    }

    [Fact]
    public void Validate_WithDuplicateTopLevelDirectoryNames_ReturnsSiblingError()
    {
        ValidationResult result =
            _service.Validate(
                CreateBootstrap(
                    CreateProfile(
                        "Chris",
                        "CareerOS_Chris",
                        "CareerProfessional")),
                CreateTemplates(
                    CreateTemplate(
                        "CareerProfessional",
                        new DirectoryNode { Name = "Resume" },
                        new DirectoryNode { Name = "resume" })));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "DUPLICATE_SIBLING_DIRECTORY");

        Assert.Equal(
            "Templates[0].Directories[1].Name",
            error.PropertyName);
    }

    [Fact]
    public void Validate_WithDuplicateNestedDirectoryNames_ReturnsSiblingError()
    {
        ValidationResult result =
            _service.Validate(
                CreateBootstrap(
                    CreateProfile(
                        "Chris",
                        "CareerOS_Chris",
                        "CareerProfessional")),
                CreateTemplates(
                    CreateTemplate(
                        "CareerProfessional",
                        new DirectoryNode
                        {
                            Name = "Resume",
                            Children =
                            [
                                new DirectoryNode { Name = "Master" },
                                new DirectoryNode { Name = "master" }
                            ]
                        })));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "DUPLICATE_SIBLING_DIRECTORY");

        Assert.Equal(
            "Templates[0].Directories[0].Children[1].Name",
            error.PropertyName);
    }

    [Fact]
    public void Validate_SameDirectoryNameUnderDifferentParents_IsAllowed()
    {
        ValidationResult result =
            _service.Validate(
                CreateBootstrap(
                    CreateProfile(
                        "Chris",
                        "CareerOS_Chris",
                        "CareerProfessional")),
                CreateTemplates(
                    CreateTemplate(
                        "CareerProfessional",
                        new DirectoryNode
                        {
                            Name = "Resume",
                            Children =
                            [
                                new DirectoryNode { Name = "Archived" }
                            ]
                        },
                        new DirectoryNode
                        {
                            Name = "Applications",
                            Children =
                            [
                                new DirectoryNode { Name = "Archived" }
                            ]
                        })));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithDeepInvalidDirectoryName_ReturnsRecursivePropertyPath()
    {
        ValidationResult result =
            _service.Validate(
                CreateBootstrap(
                    CreateProfile(
                        "Chris",
                        "CareerOS_Chris",
                        "CareerProfessional")),
                CreateTemplates(
                    CreateTemplate(
                        "CareerProfessional",
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
                                            Name = "Bad:Archive"
                                        }
                                    ]
                                }
                            ]
                        })));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "DIRECTORY_NAME_INVALID");

        Assert.Equal(
            "Templates[0].Directories[0].Children[0].Children[0].Name",
            error.PropertyName);
    }

    [Fact]
    public void Validate_WithMultipleIndependentErrors_ReturnsAllErrors()
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    string.Empty,
                    string.Empty,
                    "MissingTemplate"),
                CreateProfile(
                    "Chris",
                    "CareerOS_Chris",
                    "MissingTemplate"),
                CreateProfile(
                    "chris",
                    "careeros_chris",
                    "MissingTemplate"));

        ValidationResult result =
            _service.Validate(
                bootstrap,
                CreateTemplates(
                    CreateTemplate("CareerProfessional")));

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.Code == "PROFILE_NAME_REQUIRED");

        Assert.Contains(
            result.Errors,
            error => error.Code == "PROFILE_DIRECTORY_REQUIRED");

        Assert.Contains(
            result.Errors,
            error => error.Code == "DUPLICATE_PROFILE_NAME");

        Assert.Contains(
            result.Errors,
            error => error.Code == "DUPLICATE_PROFILE_DIRECTORY");

        Assert.Equal(
            3,
            result.Errors.Count(
                error => error.Code == "PROFILE_TEMPLATE_NOT_FOUND"));
    }

    [Fact]
    public void Validate_WithNullBootstrapConfiguration_ThrowsArgumentNullException()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => _service.Validate(
                    null!,
                    CreateTemplates(
                        CreateTemplate("CareerProfessional"))));

        Assert.Equal(
            "bootstrapConfiguration",
            exception.ParamName);
    }

    [Fact]
    public void Validate_WithNullTemplateConfiguration_ThrowsArgumentNullException()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => _service.Validate(
                    CreateBootstrap(),
                    null!));

        Assert.Equal(
            "templateConfiguration",
            exception.ParamName);
    }

    [Fact]
    public void Validate_WithMissingDestinationRoot_ReturnsDestinationRootRequiredError()
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    "Chris",
                    "CareerOS_Chris",
                    "CareerProfessional"));

        bootstrap.DestinationRoot = string.Empty;

        ValidationResult result =
            _service.Validate(
                bootstrap,
                CreateTemplates(
                    CreateTemplate("CareerProfessional")));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "DESTINATION_ROOT_REQUIRED");

        Assert.False(result.IsValid);
        Assert.Equal(
            "DestinationRoot",
            error.PropertyName);
    }

    [Fact]
    public void Validate_WithRelativeDestinationRoot_ReturnsFullyQualifiedError()
    {
        BootstrapConfiguration bootstrap =
            CreateBootstrap(
                CreateProfile(
                    "Chris",
                    "CareerOS_Chris",
                    "CareerProfessional"));

        bootstrap.DestinationRoot = "CareerOS";

        ValidationResult result =
            _service.Validate(
                bootstrap,
                CreateTemplates(
                    CreateTemplate("CareerProfessional")));

        ValidationError error =
            Assert.Single(
                result.Errors,
                error =>
                    error.Code ==
                    "DESTINATION_ROOT_NOT_FULLY_QUALIFIED");

        Assert.False(result.IsValid);
        Assert.Equal(
            "DestinationRoot",
            error.PropertyName);
    }

    [Fact]
    public void ValidateDestinationRoot_WithFullyQualifiedPath_ReturnsValidResult()
    {
        string destinationRoot =
            Path.Combine(
                Path.GetTempPath(),
                "CareerOS");

        ValidationResult result =
            _service.ValidateDestinationRoot(
                destinationRoot);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void ValidateDestinationRoot_WithMissingPath_ReturnsRequiredError(
        string destinationRoot)
    {
        ValidationResult result =
            _service.ValidateDestinationRoot(
                destinationRoot);

        ValidationError error =
            Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Equal("DESTINATION_ROOT_REQUIRED", error.Code);
        Assert.Equal("DestinationRoot", error.PropertyName);
    }

    [Theory]
    [InlineData("CareerOS")]
    [InlineData(@"Workspace\CareerOS")]
    public void ValidateDestinationRoot_WithRelativePath_ReturnsFullyQualifiedError(
        string destinationRoot)
    {
        ValidationResult result =
            _service.ValidateDestinationRoot(
                destinationRoot);

        ValidationError error =
            Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Equal(
            "DESTINATION_ROOT_NOT_FULLY_QUALIFIED",
            error.Code);
        Assert.Equal(
            "DestinationRoot",
            error.PropertyName);
    }

    [Fact]
    public void ValidateDestinationRoot_WithReservedDirectorySegment_ReturnsInvalidError()
    {
        string destinationRoot =
            Path.Combine(
                Path.GetTempPath(),
                "CON",
                "CareerOS");

        ValidationResult result =
            _service.ValidateDestinationRoot(
                destinationRoot);

        ValidationError error =
            Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Equal(
            "DESTINATION_ROOT_INVALID",
            error.Code);
        Assert.Equal(
            "DestinationRoot",
            error.PropertyName);
    }

    [Fact]
    public void ValidateDestinationRoot_DoesNotRequireDirectoryToExist()
    {
        string destinationRoot =
            Path.Combine(
                Path.GetTempPath(),
                "CareerOS.Bootstrap.Tests",
                Guid.NewGuid().ToString("N"),
                "FutureWorkspace");

        Assert.False(
            Directory.Exists(destinationRoot));

        ValidationResult result =
            _service.ValidateDestinationRoot(
                destinationRoot);

        Assert.True(result.IsValid);
        Assert.False(
            Directory.Exists(destinationRoot));
    }

    [Fact]
    public void ValidatePlannedPaths_WithPathsUnderDestinationRoot_ReturnsValidResult()
    {
        string destinationRoot =
            Path.Combine(
                Path.GetTempPath(),
                "CareerOS");

        string profileRoot =
            Path.Combine(
                destinationRoot,
                "CareerOS_Chris");

        ValidationResult result =
            _service.ValidatePlannedPaths(
                destinationRoot,
                [
                    profileRoot,
                    Path.Combine(
                        profileRoot,
                        "Resume"),
                    Path.Combine(
                        profileRoot,
                        "Resume",
                        "Master")
                ]);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidatePlannedPaths_WithDestinationRootItself_ReturnsValidResult()
    {
        string destinationRoot =
            Path.Combine(
                Path.GetTempPath(),
                "CareerOS");

        ValidationResult result =
            _service.ValidatePlannedPaths(
                destinationRoot,
                [destinationRoot]);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidatePlannedPaths_WithSiblingPrefixPath_ReturnsOutsideRootError()
    {
        string parent =
            Path.Combine(
                Path.GetTempPath(),
                "CareerOS.Bootstrap.Tests",
                Guid.NewGuid().ToString("N"));

        string destinationRoot =
            Path.Combine(
                parent,
                "CareerOS");

        string siblingPath =
            Path.Combine(
                parent,
                "CareerOS-Outside",
                "Resume");

        ValidationResult result =
            _service.ValidatePlannedPaths(
                destinationRoot,
                [siblingPath]);

        ValidationError error =
            Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Equal(
            "PLANNED_PATH_OUTSIDE_DESTINATION_ROOT",
            error.Code);
        Assert.Equal(
            "PlannedPaths[0]",
            error.PropertyName);
    }

    [Fact]
    public void ValidatePlannedPaths_WithParentTraversalEscape_ReturnsOutsideRootError()
    {
        string parent =
            Path.Combine(
                Path.GetTempPath(),
                "CareerOS.Bootstrap.Tests",
                Guid.NewGuid().ToString("N"));

        string destinationRoot =
            Path.Combine(
                parent,
                "CareerOS");

        string escapingPath =
            Path.Combine(
                destinationRoot,
                "..",
                "Outside",
                "Resume");

        ValidationResult result =
            _service.ValidatePlannedPaths(
                destinationRoot,
                [escapingPath]);

        ValidationError error =
            Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Equal(
            "PLANNED_PATH_OUTSIDE_DESTINATION_ROOT",
            error.Code);
    }

    [Fact]
    public void ValidatePlannedPaths_WithRelativePath_ReturnsFullyQualifiedError()
    {
        string destinationRoot =
            Path.Combine(
                Path.GetTempPath(),
                "CareerOS");

        ValidationResult result =
            _service.ValidatePlannedPaths(
                destinationRoot,
                [Path.Combine("CareerOS_Chris", "Resume")]);

        ValidationError error =
            Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Equal(
            "PLANNED_PATH_NOT_FULLY_QUALIFIED",
            error.Code);
        Assert.Equal(
            "PlannedPaths[0]",
            error.PropertyName);
    }

    [Fact]
    public void ValidatePlannedPaths_WithMissingPath_ReturnsRequiredError()
    {
        string destinationRoot =
            Path.Combine(
                Path.GetTempPath(),
                "CareerOS");

        ValidationResult result =
            _service.ValidatePlannedPaths(
                destinationRoot,
                [string.Empty]);

        ValidationError error =
            Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Equal(
            "PLANNED_PATH_REQUIRED",
            error.Code);
        Assert.Equal(
            "PlannedPaths[0]",
            error.PropertyName);
    }

    [Fact]
    public void ValidatePlannedPaths_WithInvalidDestinationRoot_ReturnsRootErrorBeforePathValidation()
    {
        ValidationResult result =
            _service.ValidatePlannedPaths(
                "relative-root",
                ["relative-plan"]);

        ValidationError error =
            Assert.Single(result.Errors);

        Assert.False(result.IsValid);
        Assert.Equal(
            "DESTINATION_ROOT_NOT_FULLY_QUALIFIED",
            error.Code);
        Assert.Equal(
            "DestinationRoot",
            error.PropertyName);
    }

    [Fact]
    public void ValidatePlannedPaths_WithNullCollection_ThrowsArgumentNullException()
    {
        string destinationRoot =
            Path.Combine(
                Path.GetTempPath(),
                "CareerOS");

        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => _service.ValidatePlannedPaths(
                    destinationRoot,
                    null!));

        Assert.Equal(
            "plannedPaths",
            exception.ParamName);
    }

    [Fact]
    public void ValidatePlannedPaths_DoesNotCreateFilesystemEntries()
    {
        string parent =
            Path.Combine(
                Path.GetTempPath(),
                "CareerOS.Bootstrap.Tests",
                Guid.NewGuid().ToString("N"));

        string destinationRoot =
            Path.Combine(
                parent,
                "CareerOS");

        string plannedPath =
            Path.Combine(
                destinationRoot,
                "CareerOS_Chris",
                "Resume");

        Assert.False(
            Directory.Exists(destinationRoot));
        Assert.False(
            Directory.Exists(plannedPath));

        ValidationResult result =
            _service.ValidatePlannedPaths(
                destinationRoot,
                [plannedPath]);

        Assert.True(result.IsValid);
        Assert.False(
            Directory.Exists(destinationRoot));
        Assert.False(
            Directory.Exists(plannedPath));
    }

    private static BootstrapConfiguration CreateBootstrap(
        params ProfileConfiguration[] profiles)
    {
        return new BootstrapConfiguration
        {
            DestinationRoot =
                Path.Combine(
                    Path.GetTempPath(),
                    "CareerOS"),
            Profiles = [.. profiles]
        };
    }

    private static TemplateConfiguration CreateTemplates(
        params CareerTemplate[] templates)
    {
        return new TemplateConfiguration
        {
            Templates = [.. templates]
        };
    }

    private static ProfileConfiguration CreateProfile(
        string name,
        string directory,
        string template)
    {
        return new ProfileConfiguration
        {
            Name = name,
            Directory = directory,
            Template = template
        };
    }

    private static CareerTemplate CreateTemplate(
        string name,
        params DirectoryNode[] directories)
    {
        return new CareerTemplate
        {
            Name = name,
            Directories = [.. directories]
        };
    }
}
