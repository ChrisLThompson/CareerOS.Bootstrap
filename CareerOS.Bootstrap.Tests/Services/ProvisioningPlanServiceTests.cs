using CareerOS.Bootstrap.Models;
using CareerOS.Bootstrap.Services;
using CareerOS.Bootstrap.Tests.Fixtures;

namespace CareerOS.Bootstrap.Tests.Services;

public class ProvisioningPlanServiceTests
{
    private readonly ProvisioningPlanService _service =
        new();

    [Fact]
    public void BuildPlan_WithNullPaths_ThrowsArgumentNullException()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => _service.BuildPlan(null!));

        Assert.Equal(
            "plannedPaths",
            exception.ParamName);
    }

    [Fact]
    public void BuildPlan_WithNoPaths_ReturnsEmptyPlan()
    {
        ProvisioningPlan result =
            _service.BuildPlan([]);

        Assert.Empty(result.Actions);
    }

    [Fact]
    public void BuildPlan_WithMissingDirectory_ClassifiesCreateWithoutWriting()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string target =
            fixture.GetPath(
                "Workspace",
                "CareerOS_Chris",
                "Resume");

        Assert.False(
            Directory.Exists(target));
        Assert.False(
            File.Exists(target));

        ProvisioningPlan result =
            _service.BuildPlan(
                [target]);

        ProvisioningAction action =
            Assert.Single(result.Actions);

        Assert.Equal(
            Path.GetFullPath(target),
            action.TargetPath);
        Assert.Equal(
            ProvisioningDesiredState.Directory,
            action.DesiredState);
        Assert.Equal(
            ProvisioningCurrentState.Missing,
            action.CurrentState);
        Assert.Equal(
            ProvisioningActionType.Create,
            action.ActionType);
        Assert.Equal(
            "Expected directory is missing.",
            action.Reason);
        Assert.Empty(action.Warnings);

        Assert.False(
            Directory.Exists(target));
        Assert.False(
            File.Exists(target));
    }

    [Fact]
    public void BuildPlan_WithExistingDirectory_ClassifiesPreserve()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string target =
            fixture.CreateDirectory(
                "Workspace",
                "CareerOS_Chris",
                "Resume");

        ProvisioningPlan result =
            _service.BuildPlan(
                [target]);

        ProvisioningAction action =
            Assert.Single(result.Actions);

        Assert.Equal(
            ProvisioningCurrentState.Directory,
            action.CurrentState);
        Assert.Equal(
            ProvisioningActionType.Preserve,
            action.ActionType);
        Assert.Equal(
            "Expected directory already exists.",
            action.Reason);

        string warning =
            Assert.Single(action.Warnings);

        Assert.Equal(
            "Existing directory will not be modified.",
            warning);

        Assert.True(
            Directory.Exists(target));
    }

    [Fact]
    public void BuildPlan_WithExistingFile_ClassifiesConflictWithoutModifyingFile()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        const string expectedContent =
            "existing user content";

        string target =
            fixture.CreateFile(
                Path.Combine(
                    "Workspace",
                    "CareerOS_Chris",
                    "Resume"),
                expectedContent);

        ProvisioningPlan result =
            _service.BuildPlan(
                [target]);

        ProvisioningAction action =
            Assert.Single(result.Actions);

        Assert.Equal(
            ProvisioningCurrentState.File,
            action.CurrentState);
        Assert.Equal(
            ProvisioningActionType.Conflict,
            action.ActionType);
        Assert.Equal(
            "A file exists where a directory is required.",
            action.Reason);
        Assert.Empty(action.Warnings);

        Assert.True(
            File.Exists(target));
        Assert.Equal(
            expectedContent,
            File.ReadAllText(target));
    }

    [Fact]
    public void BuildPlan_WithMixedFilesystemState_PreservesInputOrder()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string existingDirectory =
            fixture.CreateDirectory(
                "Workspace",
                "Existing");

        string missingDirectory =
            fixture.GetPath(
                "Workspace",
                "Missing");

        string conflictingFile =
            fixture.CreateFile(
                Path.Combine(
                    "Workspace",
                    "Conflict"),
                "content");

        ProvisioningPlan result =
            _service.BuildPlan(
                [
                    existingDirectory,
                    missingDirectory,
                    conflictingFile
                ]);

        Assert.Equal(
            3,
            result.Actions.Count);

        Assert.Collection(
            result.Actions,
            action =>
            {
                Assert.Equal(
                    ProvisioningActionType.Preserve,
                    action.ActionType);
                Assert.Equal(
                    Path.GetFullPath(existingDirectory),
                    action.TargetPath);
            },
            action =>
            {
                Assert.Equal(
                    ProvisioningActionType.Create,
                    action.ActionType);
                Assert.Equal(
                    Path.GetFullPath(missingDirectory),
                    action.TargetPath);
            },
            action =>
            {
                Assert.Equal(
                    ProvisioningActionType.Conflict,
                    action.ActionType);
                Assert.Equal(
                    Path.GetFullPath(conflictingFile),
                    action.TargetPath);
            });
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void BuildPlan_WithMissingPath_ClassifiesReject(
        string plannedPath)
    {
        ProvisioningPlan result =
            _service.BuildPlan(
                [plannedPath]);

        ProvisioningAction action =
            Assert.Single(result.Actions);

        Assert.Equal(
            ProvisioningCurrentState.Invalid,
            action.CurrentState);
        Assert.Equal(
            ProvisioningActionType.Reject,
            action.ActionType);
        Assert.Equal(
            "Planned path is required.",
            action.Reason);
    }

    [Fact]
    public void BuildPlan_WithRelativePath_ClassifiesReject()
    {
        string relativePath =
            Path.Combine(
                "CareerOS",
                "CareerOS_Chris");

        ProvisioningPlan result =
            _service.BuildPlan(
                [relativePath]);

        ProvisioningAction action =
            Assert.Single(result.Actions);

        Assert.Equal(
            relativePath,
            action.TargetPath);
        Assert.Equal(
            ProvisioningCurrentState.Invalid,
            action.CurrentState);
        Assert.Equal(
            ProvisioningActionType.Reject,
            action.ActionType);
        Assert.Equal(
            "Planned path must be fully qualified.",
            action.Reason);
    }

    [Fact]
    public void BuildPlan_InspectionDoesNotCreateAnyMissingDirectories()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string first =
            fixture.GetPath(
                "Workspace",
                "CareerOS_Chris");

        string second =
            fixture.GetPath(
                "Workspace",
                "CareerOS_Chris",
                "Resume");

        string third =
            fixture.GetPath(
                "Workspace",
                "CareerOS_Chris",
                "Resume",
                "Master");

        ProvisioningPlan result =
            _service.BuildPlan(
                [
                    first,
                    second,
                    third
                ]);

        Assert.All(
            result.Actions,
            action =>
            {
                Assert.Equal(
                    ProvisioningActionType.Create,
                    action.ActionType);
                Assert.Equal(
                    ProvisioningCurrentState.Missing,
                    action.CurrentState);
            });

        Assert.False(
            Directory.Exists(first));
        Assert.False(
            Directory.Exists(second));
        Assert.False(
            Directory.Exists(third));
    }

    [Fact]
    public void BuildPlan_RepeatedInspection_IsDeterministicAndReadOnly()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string existingDirectory =
            fixture.CreateDirectory(
                "Workspace",
                "Existing");

        string missingDirectory =
            fixture.GetPath(
                "Workspace",
                "Missing");

        string[] paths =
        [
            existingDirectory,
            missingDirectory
        ];

        ProvisioningPlan first =
            _service.BuildPlan(paths);

        ProvisioningPlan second =
            _service.BuildPlan(paths);

        Assert.Equal(
            first.Actions.Count,
            second.Actions.Count);

        for (int index = 0;
             index < first.Actions.Count;
             index++)
        {
            ProvisioningAction firstAction =
                first.Actions[index];

            ProvisioningAction secondAction =
                second.Actions[index];

            Assert.Equal(
                firstAction.TargetPath,
                secondAction.TargetPath);
            Assert.Equal(
                firstAction.CurrentState,
                secondAction.CurrentState);
            Assert.Equal(
                firstAction.ActionType,
                secondAction.ActionType);
            Assert.Equal(
                firstAction.Reason,
                secondAction.Reason);
        }

        Assert.True(
            Directory.Exists(existingDirectory));
        Assert.False(
            Directory.Exists(missingDirectory));
    }
}
