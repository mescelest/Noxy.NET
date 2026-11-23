using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Noxy.NET.Test.Shared.Fakes;
using Noxy.NET.UI.Components.Elements;

namespace Noxy.NET.Test.Shared;

public class DialogRenderTest
{
    private static async Task<string> RenderAsync(Dictionary<string, object?> parameterMap)
    {
        await using ServiceProvider services = new ServiceCollection().AddLogging().AddSingleton<IJSRuntime, FakeJSRuntime>().BuildServiceProvider();
        await using HtmlRenderer renderer = new(services, NullLoggerFactory.Instance);

        parameterMap[nameof(Dialog.ChildContent)] = (RenderFragment)(builder => builder.AddMarkupContent(0, "<p>the content</p>"));

        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var root = await renderer.RenderComponentAsync<Dialog>(ParameterView.FromDictionary(parameterMap));
            return root.ToHtmlString();
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public async Task ADialogThatIsNotRequired_HasACloseButton_AheadOfItsContent(bool? isRequired)
    {
        string html = await RenderAsync(new() { [nameof(Dialog.IsRequired)] = isRequired });

        Assert.Contains("aria-label=\"Close\"", html);
        Assert.True(html.IndexOf("aria-label=\"Close\"", StringComparison.Ordinal) < html.IndexOf("the content", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ARequiredDialog_HasNoCloseButton_ButStillShowsItsContent()
    {
        string html = await RenderAsync(new() { [nameof(Dialog.IsRequired)] = true });

        Assert.DoesNotContain("aria-label=\"Close\"", html);
        Assert.DoesNotContain("<button", html);
        Assert.Contains("the content", html);
    }

    [Fact]
    public async Task TheCloseButton_CanBeNamedAndFilledByTheOwner()
    {
        string html = await RenderAsync(new()
        {
            [nameof(Dialog.CloseLabel)] = "Sluiten",
            [nameof(Dialog.CloseContent)] = (RenderFragment)(builder => builder.AddMarkupContent(0, "<i>x-icon</i>"))
        });

        Assert.Contains("aria-label=\"Sluiten\"", html);
        Assert.Contains("<i>x-icon</i>", html);
        Assert.DoesNotContain("aria-label=\"Close\"", html);
    }
}
