using Noxy.NET.Models;
using Noxy.NET.UI.Components.Containers;

namespace Noxy.NET.UI.Models;

// The state of a wizard: which step is shown (CurrentIndex) and how far the user has got (ProgressIndex).
// The steps up to ProgressIndex are completed, the step at ProgressIndex is the one still to do, the rest are not reachable yet.
public class WizardContext
{
    private readonly List<WizardStep> _stepList = [];

    public int CurrentIndex { get; private set; }
    public int ProgressIndex { get; private set; }

    public int StepCount => _stepList.Count;
    public bool IsCompleted => StepCount > 0 && ProgressIndex >= StepCount;
    public WizardStep? CurrentStep => GetStep(CurrentIndex);

    // The step that was shown changed.
    public event EventHandler<GenericEventArgs<int>>? StepChanged;

    // The last step was completed.
    public event EventHandler? WizardCompleted;

    // Anything the steps show changed: the current step, the progress, or which steps there are.
    public event EventHandler? StateChanged;

    public WizardStep? GetStep(int index)
    {
        return index >= 0 && index < _stepList.Count ? _stepList[index] : null;
    }

    public int IndexOf(WizardStep step)
    {
        return _stepList.IndexOf(step);
    }

    public bool IsStepCurrent(WizardStep step) => IndexOf(step) == CurrentIndex;

    public bool IsStepActive(WizardStep step) => IndexOf(step) == ProgressIndex;

    public bool IsStepCompleted(WizardStep step)
    {
        int index = IndexOf(step);
        return index > -1 && index < ProgressIndex;
    }

    // Whether the user may go to the step: the ones done and the one still to do.
    public bool IsStepReachable(int index)
    {
        return index >= 0 && index < StepCount && index <= ProgressIndex;
    }

    // The step could not be completed (for example its form is invalid): the progress goes back to it, so the steps after it must be done again.
    public void FailStep()
    {
        ProgressIndex = CurrentIndex;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    // The step was completed: the progress moves on to the next step and shows it. Completing an earlier step again moves the progress back to the step after it, like FailStep, so the later steps are checked again.
    public void CompleteStep()
    {
        if (StepCount == 0) return;

        ProgressIndex = CurrentIndex + 1;
        if (ProgressIndex < StepCount)
        {
            GoToStep(ProgressIndex, true);
            return;
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
        WizardCompleted?.Invoke(this, EventArgs.Empty);
    }

    // Shows the step. Without ignoreProgress only the steps the user has reached can be shown.
    public void GoToStep(int step, bool ignoreProgress = false)
    {
        if (step < 0 || step >= StepCount || step == CurrentIndex) return;
        if (!ignoreProgress && step > ProgressIndex) return;

        CurrentIndex = step;
        StepChanged?.Invoke(this, new(step));
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    // The steps register themselves in the order they are rendered, so a step added in the middle later takes that place.
    public void AddStep(WizardStep step)
    {
        _stepList.Add(step);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveStep(WizardStep step)
    {
        if (!_stepList.Remove(step)) return;

        // The indexes must keep pointing at steps that exist.
        CurrentIndex = Math.Clamp(CurrentIndex, 0, Math.Max(0, StepCount - 1));
        ProgressIndex = Math.Min(ProgressIndex, StepCount);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
