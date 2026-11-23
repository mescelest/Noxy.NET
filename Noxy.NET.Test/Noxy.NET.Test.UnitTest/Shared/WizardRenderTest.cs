using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Noxy.NET.UI.Components.Containers;
using Noxy.NET.UI.Models;

namespace Noxy.NET.Test.Shared;

public class WizardRenderTest
{
    private static RenderFragment Steps()
    {
        return builder =>
        {
            AddStep(builder, 0, "First", "The first one", "first-content");
            AddStep(builder, 10, "Second", null, "second-content");
            AddStep(builder, 20, "Third", null, "third-content");
        };
    }

    private static void AddStep(RenderTreeBuilder builder, int sequence, string title, string? subtitle, string content)
    {
        builder.OpenComponent<WizardStep>(sequence);
        builder.AddComponentParameter(sequence + 1, nameof(WizardStep.Title), title);
        builder.AddComponentParameter(sequence + 2, nameof(WizardStep.Subtitle), subtitle);
        builder.AddComponentParameter(sequence + 3, nameof(WizardStep.ChildContent), (RenderFragment)(b => b.AddMarkupContent(0, $"<p>{content}</p>")));
        builder.CloseComponent();
    }

    private static async Task<string> RenderAsync(WizardContext context, Action? between = null)
    {
        await using ServiceProvider services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using HtmlRenderer renderer = new(services, NullLoggerFactory.Instance);

        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            ParameterView parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(Wizard.Context)] = context,
                [nameof(Wizard.Label)] = "Setup",
                [nameof(Wizard.ChildContent)] = Steps()
            });

            var root = await renderer.RenderComponentAsync<Wizard>(parameters);
            between?.Invoke();
            await Task.Yield();
            return root.ToHtmlString();
        });
    }

    [Fact]
    public async Task TheStepsAreAListOfButtons_AndOnlyTheCurrentStepsContentIsShown()
    {
        string html = await RenderAsync(new());

        Assert.Contains("<ol class=\"steps\" aria-label=\"Setup\"", html);
        Assert.Equal(3, html.Split("<li ").Length - 1);
        Assert.Equal(3, html.Split("<button ").Length - 1);
        Assert.Contains("first-content", html);
        Assert.DoesNotContain("second-content", html);
        Assert.DoesNotContain("third-content", html);
    }

    [Fact]
    public async Task TheFirstStepIsActiveAndCurrent_AndTheOthersAreLocked()
    {
        string html = await RenderAsync(new());

        Assert.Single(html.Split("aria-current=\"step\"").Skip(1));
        Assert.Contains("data-state=\"active\"", html);
        Assert.Equal(2, html.Split("data-state=\"pending\"").Length - 1);
        Assert.Equal(2, html.Split("disabled").Length - 1);
    }

    [Fact]
    public async Task AfterCompletingAStep_ItIsCompleted_AndTheNextStepsContentIsShown()
    {
        WizardContext context = new();

        string html = await RenderAsync(context, context.CompleteStep);

        Assert.Contains("data-state=\"completed\"", html);
        Assert.Contains("Completed</span>", html);
        Assert.Contains("second-content", html);
        Assert.DoesNotContain("first-content", html);
    }

    [Fact]
    public async Task TheCompleteHandlerOfTheApp_IsCalled_WhenTheLastStepIsCompleted()
    {
        WizardContext context = new();
        await using ServiceProvider services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using HtmlRenderer renderer = new(services, NullLoggerFactory.Instance);
        int calledCount = 0;

        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            ParameterView parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(Wizard.Context)] = context,
                [nameof(Wizard.ChildContent)] = Steps(),
                [nameof(Wizard.OnWizardComplete)] = EventCallback.Factory.Create(this, () => calledCount++)
            });

            await renderer.RenderComponentAsync<Wizard>(parameters);
            context.CompleteStep();
            context.CompleteStep();
            context.CompleteStep();
            await Task.Yield();
        });

        Assert.Equal(1, calledCount);
    }
}
