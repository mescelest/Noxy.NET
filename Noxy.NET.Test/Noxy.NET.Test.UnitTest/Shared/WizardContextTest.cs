using Noxy.NET.UI.Components.Containers;
using Noxy.NET.UI.Models;

namespace Noxy.NET.Test.Shared;

public class WizardContextTest
{
    private static (WizardContext Context, List<WizardStep> StepList) Create(int stepCount)
    {
        WizardContext context = new();
        List<WizardStep> stepList = [];
        for (int i = 0; i < stepCount; i++)
        {
            WizardStep step = new() { Context = context };
            context.AddStep(step);
            stepList.Add(step);
        }

        return (context, stepList);
    }

    #region -- Going to a step --

    [Fact]
    public void ANewWizard_ShowsTheFirstStep_AndHasMadeNoProgress()
    {
        (WizardContext context, List<WizardStep> stepList) = Create(3);

        Assert.Equal(0, context.CurrentIndex);
        Assert.Equal(0, context.ProgressIndex);
        Assert.Same(stepList[0], context.CurrentStep);
        Assert.False(context.IsCompleted);
    }

    [Fact]
    public void GoToStep_RefusesAStepBeyondTheProgress()
    {
        (WizardContext context, _) = Create(3);

        context.GoToStep(2);

        Assert.Equal(0, context.CurrentIndex);
    }

    [Fact]
    public void GoToStep_AllowsAStepBeyondTheProgress_WhenTheProgressIsIgnored()
    {
        (WizardContext context, _) = Create(3);

        context.GoToStep(2, true);

        Assert.Equal(2, context.CurrentIndex);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(99)]
    public void GoToStep_RefusesAStepThatDoesNotExist_EvenWhenTheProgressIsIgnored(int step)
    {
        (WizardContext context, _) = Create(3);

        context.GoToStep(step, true);

        Assert.Equal(0, context.CurrentIndex);
    }

    [Fact]
    public void GoToStep_RaisesStepChanged_OnlyWhenTheStepChanges()
    {
        (WizardContext context, _) = Create(3);
        List<int> raisedList = [];
        context.StepChanged += (_, e) => raisedList.Add(e.Value);
        context.CompleteStep();

        context.GoToStep(0);
        context.GoToStep(0);

        Assert.Equal([1, 0], raisedList);
    }

    #endregion -- Going to a step --

    #region -- Completing and failing --

    [Fact]
    public void CompleteStep_MovesTheProgressOn_AndShowsTheNextStep()
    {
        (WizardContext context, _) = Create(3);

        context.CompleteStep();

        Assert.Equal(1, context.CurrentIndex);
        Assert.Equal(1, context.ProgressIndex);
    }

    [Fact]
    public void CompleteStep_OnTheLastStep_CompletesTheWizard()
    {
        (WizardContext context, _) = Create(2);
        int completedCount = 0;
        context.WizardCompleted += (_, _) => completedCount++;

        context.CompleteStep();
        Assert.False(context.IsCompleted);
        Assert.Equal(0, completedCount);

        context.CompleteStep();

        Assert.True(context.IsCompleted);
        Assert.Equal(1, completedCount);
        Assert.Equal(1, context.CurrentIndex);
    }

    [Fact]
    public void CompleteStep_WithoutSteps_DoesNothing()
    {
        WizardContext context = new();
        bool isRaised = false;
        context.WizardCompleted += (_, _) => isRaised = true;
        context.StateChanged += (_, _) => isRaised = true;

        context.CompleteStep();

        Assert.False(isRaised);
        Assert.False(context.IsCompleted);
    }

    [Fact]
    public void CompleteStep_OnAnEarlierStep_MovesTheProgressBack_SoTheLaterStepsAreCheckedAgain()
    {
        (WizardContext context, _) = Create(4);
        context.CompleteStep();
        context.CompleteStep();
        context.GoToStep(0);

        context.CompleteStep();

        Assert.Equal(1, context.ProgressIndex);
        Assert.Equal(1, context.CurrentIndex);
    }

    [Fact]
    public void FailStep_MovesTheProgressBackToTheCurrentStep()
    {
        (WizardContext context, _) = Create(3);
        context.CompleteStep();
        context.CompleteStep();
        context.GoToStep(1);

        context.FailStep();

        Assert.Equal(1, context.ProgressIndex);
        Assert.Equal(1, context.CurrentIndex);
    }

    [Fact]
    public void FailStep_RaisesStateChanged_SoTheStepsAreDrawnAgain()
    {
        (WizardContext context, _) = Create(2);
        context.CompleteStep();
        int raisedCount = 0;
        context.StateChanged += (_, _) => raisedCount++;

        context.FailStep();

        Assert.Equal(1, raisedCount);
    }

    #endregion -- Completing and failing --

    #region -- The state of a step --

    [Fact]
    public void AStep_IsCompleted_BeforeTheProgress_ActiveAtIt_AndNeitherAfterIt()
    {
        (WizardContext context, List<WizardStep> stepList) = Create(3);
        context.CompleteStep();

        Assert.True(context.IsStepCompleted(stepList[0]));
        Assert.False(context.IsStepActive(stepList[0]));

        Assert.False(context.IsStepCompleted(stepList[1]));
        Assert.True(context.IsStepActive(stepList[1]));
        Assert.True(context.IsStepCurrent(stepList[1]));

        Assert.False(context.IsStepCompleted(stepList[2]));
        Assert.False(context.IsStepActive(stepList[2]));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(-1, false)]
    public void IsStepReachable_IsTrueUpToTheProgress(int index, bool expected)
    {
        (WizardContext context, _) = Create(3);

        Assert.Equal(expected, context.IsStepReachable(index));
    }

    [Fact]
    public void AStepThatIsNotInTheWizard_IsNothing()
    {
        (WizardContext context, _) = Create(2);
        WizardStep stranger = new() { Context = context };

        Assert.Equal(-1, context.IndexOf(stranger));
        Assert.False(context.IsStepCompleted(stranger));
        Assert.False(context.IsStepCurrent(stranger));
    }

    #endregion -- The state of a step --

    #region -- Steps coming and going --

    [Fact]
    public void RemoveStep_KeepsTheIndexesOnStepsThatExist()
    {
        (WizardContext context, List<WizardStep> stepList) = Create(3);
        context.CompleteStep();
        context.CompleteStep();
        Assert.Equal(2, context.CurrentIndex);

        context.RemoveStep(stepList[2]);

        Assert.Equal(1, context.CurrentIndex);
        Assert.Same(stepList[1], context.CurrentStep);
        Assert.Equal(2, context.ProgressIndex);
    }

    [Fact]
    public void RemoveStep_OfTheLastStepsLeft_LeavesAnEmptyWizard()
    {
        (WizardContext context, List<WizardStep> stepList) = Create(2);
        context.CompleteStep();

        context.RemoveStep(stepList[0]);
        context.RemoveStep(stepList[1]);

        Assert.Equal(0, context.StepCount);
        Assert.Equal(0, context.CurrentIndex);
        Assert.Equal(0, context.ProgressIndex);
        Assert.Null(context.CurrentStep);
        Assert.False(context.IsCompleted);
    }

    [Fact]
    public void RemoveStep_OfAStepThatIsNotThere_ChangesNothing()
    {
        (WizardContext context, _) = Create(2);
        int raisedCount = 0;
        context.StateChanged += (_, _) => raisedCount++;

        context.RemoveStep(new() { Context = context });

        Assert.Equal(0, raisedCount);
        Assert.Equal(2, context.StepCount);
    }

    [Fact]
    public void AddStep_AndRemoveStep_RaiseStateChanged()
    {
        WizardContext context = new();
        int raisedCount = 0;
        context.StateChanged += (_, _) => raisedCount++;
        WizardStep step = new() { Context = context };

        context.AddStep(step);
        context.RemoveStep(step);

        Assert.Equal(2, raisedCount);
    }

    #endregion -- Steps coming and going --
}
