using CareerOS.Bootstrap.Models;
using CareerOS.Bootstrap.Services;
using CareerOS.Bootstrap.Tests.Fixtures;

namespace CareerOS.Bootstrap.Tests.Integration;

public class BootstrapPlanningWorkflowTests
{
    [Fact]
    public void PlanningWorkflow_WithValidConfiguration_LoadsResolvesAndBuildsRecursivePlan()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string bootstrapPath =
            fixture.CreateFile(
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

        string templatesPath =
            fixture.CreateFile(
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
                              "children": [
                                {
                                  "name": "Archived",
                                  "children": []
                                }
                              ]
                            },
                            {
                              "name": "RC",
                              "children": []
                            }
                          ]
                        },
                        {
                          "name": "Applications",
                          "children": []
                        }
                      ]
                    }
                  ]
                }
                """);

        JsonConfigurationService configurationService =
            new();

        TemplateResolverService templateResolverService =
            new();

        DirectoryPlanService directoryPlanService =
            new();

        ConfigurationValidationService validationService =
            new();

        BootstrapConfiguration bootstrapConfiguration =
            configurationService.LoadBootstrapConfiguration(
                bootstrapPath);

        TemplateConfiguration templateConfiguration =
            configurationService.LoadTemplateConfiguration(
                templatesPath);

        ValidationResult configurationValidation =
            validationService.Validate(
                bootstrapConfiguration,
                templateConfiguration);

        Assert.True(configurationValidation.IsValid);

        ProfileConfiguration profile =
            Assert.Single(
                bootstrapConfiguration.Profiles);

        CareerTemplate template =
            templateResolverService.ResolveTemplate(
                templateConfiguration,
                profile.Template);

        string workspaceBasePath =
            fixture.GetPath(
                "Workspace");

        IReadOnlyList<string> plan =
            directoryPlanService.BuildPlan(
                workspaceBasePath,
                profile,
                template);

        ValidationResult planValidation =
            validationService.ValidatePlannedPaths(
                workspaceBasePath,
                plan);

        Assert.True(planValidation.IsValid);

        string profileRoot =
            Path.Combine(
                workspaceBasePath,
                "CareerOS_Chris");

        Assert.Equal(
            [
                profileRoot,
                Path.Combine(profileRoot, "Resume"),
                Path.Combine(profileRoot, "Resume", "Master"),
                Path.Combine(profileRoot, "Resume", "Master", "Archived"),
                Path.Combine(profileRoot, "Resume", "RC"),
                Path.Combine(profileRoot, "Applications")
            ],
            plan);
    }

    [Fact]
    public void PlanningWorkflow_WithMultipleProfiles_UsesEachAssignedTemplate()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string bootstrapPath =
            fixture.CreateFile(
                "bootstrap.json",
                """
                {
                  "profiles": [
                    {
                      "name": "Chris",
                      "directory": "CareerOS_Chris",
                      "template": "CareerProfessional"
                    },
                    {
                      "name": "Katie",
                      "directory": "CareerOS_Katie",
                      "template": "HealthcareProfessional"
                    }
                  ]
                }
                """);

        string templatesPath =
            fixture.CreateFile(
                "templates.json",
                """
                {
                  "templates": [
                    {
                      "name": "CareerProfessional",
                      "directories": [
                        {
                          "name": "Resume",
                          "children": []
                        }
                      ]
                    },
                    {
                      "name": "HealthcareProfessional",
                      "directories": [
                        {
                          "name": "Credentials",
                          "children": [
                            {
                              "name": "Licenses",
                              "children": []
                            }
                          ]
                        }
                      ]
                    }
                  ]
                }
                """);

        JsonConfigurationService configurationService =
            new();

        TemplateResolverService templateResolverService =
            new();

        DirectoryPlanService directoryPlanService =
            new();

        ConfigurationValidationService validationService =
            new();

        BootstrapConfiguration bootstrapConfiguration =
            configurationService.LoadBootstrapConfiguration(
                bootstrapPath);

        TemplateConfiguration templateConfiguration =
            configurationService.LoadTemplateConfiguration(
                templatesPath);

        ValidationResult configurationValidation =
            validationService.Validate(
                bootstrapConfiguration,
                templateConfiguration);

        Assert.True(configurationValidation.IsValid);

        string workspaceBasePath =
            fixture.GetPath(
                "Workspace");

        Dictionary<string, IReadOnlyList<string>> plans =
            [];

        foreach (ProfileConfiguration profile in bootstrapConfiguration.Profiles)
        {
            CareerTemplate template =
                templateResolverService.ResolveTemplate(
                    templateConfiguration,
                    profile.Template);

            IReadOnlyList<string> plan =
                directoryPlanService.BuildPlan(
                    workspaceBasePath,
                    profile,
                    template);

            ValidationResult planValidation =
                validationService.ValidatePlannedPaths(
                    workspaceBasePath,
                    plan);

            Assert.True(planValidation.IsValid);

            plans[profile.Name] =
                plan;
        }

        Assert.Equal(
            2,
            plans.Count);

        string chrisRoot =
            Path.Combine(
                workspaceBasePath,
                "CareerOS_Chris");

        Assert.Equal(
            [
                chrisRoot,
                Path.Combine(chrisRoot, "Resume")
            ],
            plans["Chris"]);

        string katieRoot =
            Path.Combine(
                workspaceBasePath,
                "CareerOS_Katie");

        Assert.Equal(
            [
                katieRoot,
                Path.Combine(katieRoot, "Credentials"),
                Path.Combine(katieRoot, "Credentials", "Licenses")
            ],
            plans["Katie"]);
    }

    [Fact]
    public void PlanningWorkflow_DoesNotCreatePlannedWorkspaceDirectories()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string bootstrapPath =
            fixture.CreateFile(
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

        string templatesPath =
            fixture.CreateFile(
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

        JsonConfigurationService configurationService =
            new();

        TemplateResolverService templateResolverService =
            new();

        DirectoryPlanService directoryPlanService =
            new();

        ConfigurationValidationService validationService =
            new();

        BootstrapConfiguration bootstrapConfiguration =
            configurationService.LoadBootstrapConfiguration(
                bootstrapPath);

        TemplateConfiguration templateConfiguration =
            configurationService.LoadTemplateConfiguration(
                templatesPath);

        ValidationResult configurationValidation =
            validationService.Validate(
                bootstrapConfiguration,
                templateConfiguration);

        Assert.True(configurationValidation.IsValid);

        ProfileConfiguration profile =
            Assert.Single(
                bootstrapConfiguration.Profiles);

        CareerTemplate template =
            templateResolverService.ResolveTemplate(
                templateConfiguration,
                profile.Template);

        string workspaceBasePath =
            fixture.GetPath(
                "Workspace");

        IReadOnlyList<string> plan =
            directoryPlanService.BuildPlan(
                workspaceBasePath,
                profile,
                template);

        ValidationResult planValidation =
            validationService.ValidatePlannedPaths(
                workspaceBasePath,
                plan);

        Assert.True(planValidation.IsValid);
        Assert.NotEmpty(plan);

        Assert.False(
            Directory.Exists(workspaceBasePath));

        foreach (string plannedPath in plan)
        {
            Assert.False(
                Directory.Exists(plannedPath),
                $"Planning unexpectedly created directory: {plannedPath}");
        }
    }

    [Fact]
    public void PlanningWorkflow_WithUnknownAssignedTemplate_FailsValidationBeforeResolution()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string bootstrapPath =
            fixture.CreateFile(
                "bootstrap.json",
                """
                {
                  "profiles": [
                    {
                      "name": "Chris",
                      "directory": "CareerOS_Chris",
                      "template": "UnknownTemplate"
                    }
                  ]
                }
                """);

        string templatesPath =
            fixture.CreateFile(
                "templates.json",
                """
                {
                  "templates": [
                    {
                      "name": "CareerProfessional",
                      "directories": []
                    }
                  ]
                }
                """);

        JsonConfigurationService configurationService =
            new();

        ConfigurationValidationService validationService =
            new();

        BootstrapConfiguration bootstrapConfiguration =
            configurationService.LoadBootstrapConfiguration(
                bootstrapPath);

        TemplateConfiguration templateConfiguration =
            configurationService.LoadTemplateConfiguration(
                templatesPath);

        ValidationResult result =
            validationService.Validate(
                bootstrapConfiguration,
                templateConfiguration);

        Assert.False(result.IsValid);

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "PROFILE_TEMPLATE_NOT_FOUND");

        Assert.Equal(
            "Profiles[0].Template",
            error.PropertyName);
    }

    [Fact]
    public void PlanningWorkflow_WithInvalidNestedDirectory_FailsValidationBeforePlanning()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string bootstrapPath =
            fixture.CreateFile(
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

        string templatesPath =
            fixture.CreateFile(
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
                              "name": "Bad:Directory",
                              "children": []
                            }
                          ]
                        }
                      ]
                    }
                  ]
                }
                """);

        JsonConfigurationService configurationService =
            new();

        ConfigurationValidationService validationService =
            new();

        BootstrapConfiguration bootstrapConfiguration =
            configurationService.LoadBootstrapConfiguration(
                bootstrapPath);

        TemplateConfiguration templateConfiguration =
            configurationService.LoadTemplateConfiguration(
                templatesPath);

        ValidationResult result =
            validationService.Validate(
                bootstrapConfiguration,
                templateConfiguration);

        Assert.False(result.IsValid);

        ValidationError error =
            Assert.Single(
                result.Errors,
                error => error.Code == "DIRECTORY_NAME_INVALID");

        Assert.Equal(
            "Templates[0].Directories[0].Children[0].Name",
            error.PropertyName);
    }

    [Fact]
    public void PlanningWorkflow_WithEscapingPlannedPath_FailsContainmentValidation()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string destinationRoot =
            fixture.GetPath(
                "Workspace");

        string escapingPath =
            Path.Combine(
                destinationRoot,
                "..",
                "Outside",
                "Resume");

        ConfigurationValidationService validationService =
            new();

        ValidationResult result =
            validationService.ValidatePlannedPaths(
                destinationRoot,
                [escapingPath]);

        Assert.False(result.IsValid);

        ValidationError error =
            Assert.Single(result.Errors);

        Assert.Equal(
            "PLANNED_PATH_OUTSIDE_DESTINATION_ROOT",
            error.Code);
    }

}
