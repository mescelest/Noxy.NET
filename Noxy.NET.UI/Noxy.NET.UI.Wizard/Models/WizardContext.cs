using System.Collections.ObjectModel;
using Noxy.NET.Models;
using Noxy.NET.UI.Components;

namespace Noxy.NET.UI.Models;

public class WizardContext
{
    public int CurrentIndex { get; protected set; }
    public int ProgressIndex { get; protected set; }

    protected Collection<WizardStep> StepList { get; } = [];

    public event EventHandler<GenericEventArgs<int>>? StepChanged;
    public event EventHandler? WizardCompleted;

    public void FailStep()
    {
        ProgressIndex = CurrentIndex;
    }

    public void CompleteStep()
    {
        ProgressIndex = CurrentIndex + 1;
        if (ProgressIndex < StepList.Count)
        {
            GoToStep(ProgressIndex, true);
        }
        else
        {
            WizardCompleted?.Invoke(this, new());
        }
    }

    public void GoToStep(int step, bool flagIgnoreCompletion = false)
    {
        if (step == CurrentIndex || !flagIgnoreCompletion && step > ProgressIndex) return;
        CurrentIndex = step;
        StepChanged?.Invoke(this, new(step));
    }

    internal static class Logic
    {
        public static bool HasSteps(WizardContext context)
        {
            return context.StepList.Count > 0;
        }

        public static WizardStep GetCurrentStep(WizardContext context)
        {
            return context.StepList[context.CurrentIndex];
        }

        public static IEnumerable<WizardStep> GetStepList(WizardContext context)
        {
            return [.. context.StepList];
        }

        public static void RegisterStep(WizardContext context, WizardStep step)
        {
            context.StepList.Add(step);
        }

        public static void DeregisterStep(WizardContext context, WizardStep step)
        {
            context.StepList.Remove(step);
        }

        public static bool IsWizardComplete(WizardContext context)
        {
            return context.ProgressIndex == context.StepList.Count;
        }

        public static bool IsStepCurrent(WizardContext context, WizardStep step)
        {
            return GetStepIndex(context, step) == context.CurrentIndex;
        }

        public static bool IsStepActive(WizardContext context, WizardStep step)
        {
            return GetStepIndex(context, step) == context.ProgressIndex;
        }

        public static bool IsStepCompleted(WizardContext context, WizardStep step)
        {
            return GetStepIndex(context, step) < context.ProgressIndex;
        }

        public static int GetStepIndex(WizardContext context, WizardStep step)
        {
            int index = context.StepList.IndexOf(step);
            return index > -1 ? index : throw new ArgumentException("Step is not a part of the Wizard.", nameof(step));
        }
    }
}
