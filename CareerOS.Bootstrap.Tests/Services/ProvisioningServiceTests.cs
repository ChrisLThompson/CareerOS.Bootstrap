using CareerOS.Bootstrap.Models;
using CareerOS.Bootstrap.Services;
using CareerOS.Bootstrap.Tests.Fixtures;

namespace CareerOS.Bootstrap.Tests.Services;

public class ProvisioningServiceTests
{
    private readonly ProvisioningService _service =
        new(
            new ConfigurationValidationService());

    [Fact]
    public void Provision_WithNullPlan_ThrowsArgumentNullException()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => _service.Provision(
                    fixture.RootPath,
                    null!));

        Assert.Equal(
            "plan",
            exception.ParamName);
    }

    [Fact]
    public void Provision_WithInvalidDestinationRoot_ReturnsFailureWithoutWriting()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string targetPath =
            fixture.GetPath(
                "CareerOS_Chris");

        ProvisioningResult result =
            _service.Provision(
                "relative-root",
                CreatePlan(
                    CreateAction(targetPath)));

        Assert.False(result.Succeeded);
        Assert.False(result.Validation.IsValid);
        Assert.Contains(
            result.Validation.Errors,
            error =>
                error.Code ==
                "DESTINATION_ROOT_NOT_FULLY_QUALIFIED");
        Assert.False(
            Directory.Exists(targetPath));
    }

    [Fact]
    public void Provision_WithPathOutsideDestinationRoot_ReturnsFailureWithoutWriting()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string destinationRoot =
            fixture.GetPath(
                "CareerOS");

        string validTarget =
            Path.Combine(
                destinationRoot,
                "CareerOS_Chris");

        string outsideTarget =
            Path.Combine(
                Path.GetDirectoryName(destinationRoot)!,
                "Outside",
                "CareerOS_Katie");

        ProvisioningResult result =
            _service.Provision(
                destinationRoot,
                CreatePlan(
                    CreateAction(validTarget),
                    CreateAction(outsideTarget)));

        Assert.False(result.Succeeded);
        Assert.Contains(
            result.Validation.Errors,
            error =>
                error.Code ==
                "PLANNED_PATH_OUTSIDE_DESTINATION_ROOT");

        Assert.False(
            Directory.Exists(validTarget));
        Assert.False(
            Directory.Exists(outsideTarget));
    }

    [Fact]
    public void Provision_WithCreateAction_CreatesMissingDirectory()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string destinationRoot =
            fixture.GetPath(
                "CareerOS");

        string targetPath =
            Path.Combine(
                destinationRoot,
                "CareerOS_Chris");

        ProvisioningResult result =
            _service.Provision(
                destinationRoot,
                CreatePlan(
                    CreateAction(targetPath)));

        Assert.True(result.Succeeded);
        Assert.True(result.Validation.IsValid);
        Assert.Equal(
            1,
            result.CreatedCount);
        Assert.Equal(
            0,
            result.PreservedCount);
        Assert.True(
            Directory.Exists(targetPath));
    }

    [Fact]
    public void Provision_WithNestedCreateActions_CreatesDirectories()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string destinationRoot =
            fixture.GetPath(
                "CareerOS");

        string profileRoot =
            Path.Combine(
                destinationRoot,
                "CareerOS_Chris");

        string resume =
            Path.Combine(
                profileRoot,
                "Resume");

        string master =
            Path.Combine(
                resume,
                "Master");

        ProvisioningResult result =
            _service.Provision(
                destinationRoot,
                CreatePlan(
                    CreateAction(profileRoot),
                    CreateAction(resume),
                    CreateAction(master)));

        Assert.True(result.Succeeded);
        Assert.Equal(
            3,
            result.CreatedCount);
        Assert.True(
            Directory.Exists(profileRoot));
        Assert.True(
            Directory.Exists(resume));
        Assert.True(
            Directory.Exists(master));
    }

    [Fact]
    public void Provision_WithPreserveAction_DoesNotModifyExistingDirectory()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string destinationRoot =
            fixture.CreateDirectory(
                "CareerOS");

        string existingDirectory =
            fixture.CreateDirectory(
                "CareerOS",
                "CareerOS_Chris");

        string existingFile =
            fixture.CreateFile(
                Path.Combine(
                    "CareerOS",
                    "CareerOS_Chris",
                    "resume.txt"),
                "keep me");

        DateTime originalWriteTime =
            File.GetLastWriteTimeUtc(existingFile);

        ProvisioningResult result =
            _service.Provision(
                destinationRoot,
                CreatePlan(
                    PreserveAction(existingDirectory)));

        Assert.True(result.Succeeded);
        Assert.Equal(
            0,
            result.CreatedCount);
        Assert.Equal(
            1,
            result.PreservedCount);
        Assert.Equal(
            "keep me",
            File.ReadAllText(existingFile));
        Assert.Equal(
            originalWriteTime,
            File.GetLastWriteTimeUtc(existingFile));
    }

    [Fact]
    public void Provision_WithConflictAction_ReturnsFailureWithoutModifyingFile()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string destinationRoot =
            fixture.CreateDirectory(
                "CareerOS");

        string conflictingFile =
            fixture.CreateFile(
                Path.Combine(
                    "CareerOS",
                    "CareerOS_Chris"),
                "existing content");

        ProvisioningResult result =
            _service.Provision(
                destinationRoot,
                CreatePlan(
                    ConflictAction(conflictingFile)));

        Assert.False(result.Succeeded);
        Assert.Contains(
            result.Validation.Errors,
            error =>
                error.Code ==
                "PROVISIONING_ACTION_CONFLICT");
        Assert.True(
            File.Exists(conflictingFile));
        Assert.Equal(
            "existing content",
            File.ReadAllText(conflictingFile));
    }

    [Fact]
    public void Provision_WithRejectAction_ReturnsFailureWithoutWriting()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string destinationRoot =
            fixture.GetPath(
                "CareerOS");

        string targetPath =
            Path.Combine(
                destinationRoot,
                "CareerOS_Chris");

        ProvisioningResult result =
            _service.Provision(
                destinationRoot,
                CreatePlan(
                    RejectAction(targetPath)));

        Assert.False(result.Succeeded);
        Assert.Contains(
            result.Validation.Errors,
            error =>
                error.Code ==
                "PROVISIONING_ACTION_REJECTED");
        Assert.False(
            Directory.Exists(targetPath));
    }

    [Fact]
    public void Provision_WithMixedSafeActions_CreatesMissingAndPreservesExisting()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string destinationRoot =
            fixture.CreateDirectory(
                "CareerOS");

        string existingDirectory =
            fixture.CreateDirectory(
                "CareerOS",
                "CareerOS_Chris");

        string missingDirectory =
            Path.Combine(
                existingDirectory,
                "Resume");

        ProvisioningResult result =
            _service.Provision(
                destinationRoot,
                CreatePlan(
                    PreserveAction(existingDirectory),
                    CreateAction(missingDirectory)));

        Assert.True(result.Succeeded);
        Assert.Equal(
            1,
            result.CreatedCount);
        Assert.Equal(
            1,
            result.PreservedCount);
        Assert.True(
            Directory.Exists(existingDirectory));
        Assert.True(
            Directory.Exists(missingDirectory));
    }

    [Fact]
    public void Provision_WithAnyUnsafeAction_PerformsNoWrites()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string destinationRoot =
            fixture.CreateDirectory(
                "CareerOS");

        string missingDirectory =
            Path.Combine(
                destinationRoot,
                "CareerOS_Chris");

        string conflictingFile =
            fixture.CreateFile(
                Path.Combine(
                    "CareerOS",
                    "CareerOS_Katie"),
                "existing content");

        ProvisioningResult result =
            _service.Provision(
                destinationRoot,
                CreatePlan(
                    CreateAction(missingDirectory),
                    ConflictAction(conflictingFile)));

        Assert.False(result.Succeeded);
        Assert.False(
            Directory.Exists(missingDirectory));
        Assert.True(
            File.Exists(conflictingFile));
    }

    [Fact]
    public void Provision_CreateActionThatBecameExisting_PreservesAtWriteTime()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string destinationRoot =
            fixture.CreateDirectory(
                "CareerOS");

        string targetPath =
            Path.Combine(
                destinationRoot,
                "CareerOS_Chris");

        ProvisioningPlan plan =
            CreatePlan(
                CreateAction(targetPath));

        Directory.CreateDirectory(targetPath);

        ProvisioningResult result =
            _service.Provision(
                destinationRoot,
                plan);

        Assert.True(result.Succeeded);
        Assert.Equal(
            0,
            result.CreatedCount);
        Assert.Equal(
            1,
            result.PreservedCount);
    }

    [Fact]
    public void Provision_CreateActionThatBecameFile_ReturnsWriteTimeConflictWithoutWriting()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string destinationRoot =
            fixture.CreateDirectory(
                "CareerOS");

        string targetPath =
            Path.Combine(
                destinationRoot,
                "CareerOS_Chris");

        ProvisioningPlan plan =
            CreatePlan(
                CreateAction(targetPath));

        File.WriteAllText(
            targetPath,
            "conflict");

        ProvisioningResult result =
            _service.Provision(
                destinationRoot,
                plan);

        Assert.False(result.Succeeded);
        Assert.Contains(
            result.Validation.Errors,
            error =>
                error.Code ==
                "PROVISIONING_WRITE_TIME_CONFLICT");
        Assert.True(
            File.Exists(targetPath));
        Assert.Equal(
            "conflict",
            File.ReadAllText(targetPath));
    }

    [Fact]
    public void Provision_PreserveActionThatBecameMissing_ReturnsFailureWithoutWriting()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string destinationRoot =
            fixture.CreateDirectory(
                "CareerOS");

        string targetPath =
            fixture.CreateDirectory(
                "CareerOS",
                "CareerOS_Chris");

        ProvisioningPlan plan =
            CreatePlan(
                PreserveAction(targetPath));

        Directory.Delete(targetPath);

        ProvisioningResult result =
            _service.Provision(
                destinationRoot,
                plan);

        Assert.False(result.Succeeded);
        Assert.Contains(
            result.Validation.Errors,
            error =>
                error.Code ==
                "PROVISIONING_STALE_PRESERVE_ACTION");
        Assert.False(
            Directory.Exists(targetPath));
    }

    [Fact]
    public void Provision_RepeatedExecution_IsSafe()
    {
        using TemporaryDirectoryFixture fixture =
            new();

        string destinationRoot =
            fixture.GetPath(
                "CareerOS");

        string profileRoot =
            Path.Combine(
                destinationRoot,
                "CareerOS_Chris");

        ProvisioningPlan firstPlan =
            CreatePlan(
                CreateAction(profileRoot));

        ProvisioningResult firstResult =
            _service.Provision(
                destinationRoot,
                firstPlan);

        ProvisioningPlan secondPlan =
            new ProvisioningPlanService()
                .BuildPlan(
                    [profileRoot]);

        ProvisioningResult secondResult =
            _service.Provision(
                destinationRoot,
                secondPlan);

        Assert.True(firstResult.Succeeded);
        Assert.True(secondResult.Succeeded);
        Assert.Equal(
            1,
            firstResult.CreatedCount);
        Assert.Equal(
            0,
            secondResult.CreatedCount);
        Assert.Equal(
            1,
            secondResult.PreservedCount);
        Assert.True(
            Directory.Exists(profileRoot));
    }

    private static ProvisioningPlan CreatePlan(
        params ProvisioningAction[] actions)
    {
        return new ProvisioningPlan
        {
            Actions = actions
        };
    }

    private static ProvisioningAction CreateAction(
        string targetPath)
    {
        return new ProvisioningAction
        {
            TargetPath = targetPath,
            DesiredState =
                ProvisioningDesiredState.Directory,
            CurrentState =
                ProvisioningCurrentState.Missing,
            ActionType =
                ProvisioningActionType.Create,
            Reason =
                "Expected directory is missing."
        };
    }

    private static ProvisioningAction PreserveAction(
        string targetPath)
    {
        return new ProvisioningAction
        {
            TargetPath = targetPath,
            DesiredState =
                ProvisioningDesiredState.Directory,
            CurrentState =
                ProvisioningCurrentState.Directory,
            ActionType =
                ProvisioningActionType.Preserve,
            Reason =
                "Expected directory already exists."
        };
    }

    private static ProvisioningAction ConflictAction(
        string targetPath)
    {
        return new ProvisioningAction
        {
            TargetPath = targetPath,
            DesiredState =
                ProvisioningDesiredState.Directory,
            CurrentState =
                ProvisioningCurrentState.File,
            ActionType =
                ProvisioningActionType.Conflict,
            Reason =
                "A file exists where a directory is required."
        };
    }

    private static ProvisioningAction RejectAction(
        string targetPath)
    {
        return new ProvisioningAction
        {
            TargetPath = targetPath,
            DesiredState =
                ProvisioningDesiredState.Directory,
            CurrentState =
                ProvisioningCurrentState.Invalid,
            ActionType =
                ProvisioningActionType.Reject,
            Reason =
                "Planned path is invalid."
        };
    }
}
