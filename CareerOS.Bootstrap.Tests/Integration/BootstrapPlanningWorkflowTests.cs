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

        BootstrapConfiguration bootstrapConfiguration =
            configurationService.LoadBootstrapConfiguration(
                bootstrapPath);

        TemplateConfiguration templateConfiguration =
            configurationService.LoadTemplateConfiguration(
                templatesPath);

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

        BootstrapConfiguration bootstrapConfiguration =
            configurationService.LoadBootstrapConfiguration(
                bootstrapPath);

        TemplateConfiguration templateConfiguration =
            configurationService.LoadTemplateConfiguration(
                templatesPath);

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

            plans[profile.Name] =
                directoryPlanService.BuildPlan(
                    workspaceBasePath,
                    profile,
                    template);
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

        BootstrapConfiguration bootstrapConfiguration =
            configurationService.LoadBootstrapConfiguration(
                bootstrapPath);

        TemplateConfiguration templateConfiguration =
            configurationService.LoadTemplateConfiguration(
                templatesPath);

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
    public void PlanningWorkflow_WithUnknownAssignedTemplate_FailsDuringResolution()
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

        TemplateResolverService templateResolverService =
            new();

        BootstrapConfiguration bootstrapConfiguration =
            configurationService.LoadBootstrapConfiguration(
                bootstrapPath);

        TemplateConfiguration templateConfiguration =
            configurationService.LoadTemplateConfiguration(
                templatesPath);

        ProfileConfiguration profile =
            Assert.Single(
                bootstrapConfiguration.Profiles);

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => templateResolverService.ResolveTemplate(
                    templateConfiguration,
                    profile.Template));

        Assert.Equal(
            "Template 'UnknownTemplate' was not found in the template configuration.",
            exception.Message);
    }
}
