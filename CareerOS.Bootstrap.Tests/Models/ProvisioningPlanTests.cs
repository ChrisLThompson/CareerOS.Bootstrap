using CareerOS.Bootstrap.Models;

namespace CareerOS.Bootstrap.Tests.Models;

public class ProvisioningPlanTests
{
    [Fact]
    public void Constructor_DefaultPlan_HasNoActions()
    {
        ProvisioningPlan plan = new();

        Assert.Empty(plan.Actions);
    }

    [Fact]
    public void ProvisioningAction_DefaultValues_AreSafeAndNonNull()
    {
        ProvisioningAction action = new();

        Assert.Equal(string.Empty, action.TargetPath);
        Assert.Equal(
            ProvisioningDesiredState.Directory,
            action.DesiredState);
        Assert.Equal(
            ProvisioningCurrentState.Missing,
            action.CurrentState);
        Assert.Equal(
            ProvisioningActionType.Create,
            action.ActionType);
        Assert.Equal(string.Empty, action.Reason);
        Assert.Empty(action.Warnings);
    }

    [Theory]
    [InlineData(ProvisioningActionType.Create)]
    [InlineData(ProvisioningActionType.Preserve)]
    [InlineData(ProvisioningActionType.Skip)]
    [InlineData(ProvisioningActionType.Conflict)]
    [InlineData(ProvisioningActionType.Reject)]
    public void ProvisioningAction_ActionType_PreservesAssignedClassification(
        ProvisioningActionType actionType)
    {
        ProvisioningAction action =
            new()
            {
                ActionType = actionType
            };

        Assert.Equal(
            actionType,
            action.ActionType);
    }

    [Theory]
    [InlineData(ProvisioningCurrentState.Missing)]
    [InlineData(ProvisioningCurrentState.Directory)]
    [InlineData(ProvisioningCurrentState.File)]
    [InlineData(ProvisioningCurrentState.Other)]
    [InlineData(ProvisioningCurrentState.Invalid)]
    [InlineData(ProvisioningCurrentState.Unsafe)]
    public void ProvisioningAction_CurrentState_PreservesAssignedObservedState(
        ProvisioningCurrentState currentState)
    {
        ProvisioningAction action =
            new()
            {
                CurrentState = currentState
            };

        Assert.Equal(
            currentState,
            action.CurrentState);
    }

    [Fact]
    public void ProvisioningAction_WithStructuredValues_PreservesPlanData()
    {
        string targetPath =
            Path.Combine(
                Path.GetTempPath(),
                "CareerOS",
                "CareerOS_Chris",
                "Resume");

        ProvisioningAction action =
            new()
            {
                TargetPath = targetPath,
                DesiredState =
                    ProvisioningDesiredState.Directory,
                CurrentState =
                    ProvisioningCurrentState.Directory,
                ActionType =
                    ProvisioningActionType.Preserve,
                Reason =
                    "Expected directory already exists.",
                Warnings =
                [
                    "Existing directory will not be modified."
                ]
            };

        Assert.Equal(
            targetPath,
            action.TargetPath);
        Assert.Equal(
            ProvisioningDesiredState.Directory,
            action.DesiredState);
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
    }

    [Fact]
    public void ProvisioningPlan_WithActions_PreservesTraversalOrder()
    {
        ProvisioningAction first =
            new()
            {
                TargetPath = @"D:\CareerOS\CareerOS_Chris",
                CurrentState =
                    ProvisioningCurrentState.Directory,
                ActionType =
                    ProvisioningActionType.Preserve,
                Reason =
                    "Profile root already exists."
            };

        ProvisioningAction second =
            new()
            {
                TargetPath =
                    @"D:\CareerOS\CareerOS_Chris\Resume",
                CurrentState =
                    ProvisioningCurrentState.Missing,
                ActionType =
                    ProvisioningActionType.Create,
                Reason =
                    "Expected directory is missing."
            };

        ProvisioningPlan plan =
            new()
            {
                Actions =
                [
                    first,
                    second
                ]
            };

        Assert.Equal(
            2,
            plan.Actions.Count);
        Assert.Same(
            first,
            plan.Actions[0]);
        Assert.Same(
            second,
            plan.Actions[1]);
    }

    [Fact]
    public void ProvisioningAction_Conflict_CanCarryConflictReason()
    {
        ProvisioningAction action =
            new()
            {
                TargetPath =
                    @"D:\CareerOS\CareerOS_Chris\Resume",
                CurrentState =
                    ProvisioningCurrentState.File,
                ActionType =
                    ProvisioningActionType.Conflict,
                Reason =
                    "A file exists where a directory is required."
            };

        Assert.Equal(
            ProvisioningActionType.Conflict,
            action.ActionType);
        Assert.Equal(
            ProvisioningCurrentState.File,
            action.CurrentState);
        Assert.Equal(
            "A file exists where a directory is required.",
            action.Reason);
    }
}
