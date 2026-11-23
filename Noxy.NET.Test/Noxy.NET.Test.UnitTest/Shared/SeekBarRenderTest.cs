using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Noxy.NET.UI.Components.Elements;

namespace Noxy.NET.Test.Shared;

public class SeekBarRenderTest
{
    private static async Task<string> RenderAsync(double value, double max, string culture)
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
        try
        {
            await using ServiceProvider services = new ServiceCollection().AddLogging().BuildServiceProvider();
            await using HtmlRenderer renderer = new(services, NullLoggerFactory.Instance);

            return await renderer.Dispatcher.InvokeAsync(async () =>
            {
                ParameterView parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
                {
                    [nameof(SeekBar.Value)] = value,
                    [nameof(SeekBar.Max)] = max,
                    [nameof(SeekBar.Label)] = "Position"
                });

                var root = await renderer.RenderComponentAsync<SeekBar>(parameters);
                return root.ToHtmlString();
            });
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("nl-NL")]
    public async Task TheRangeInput_GetsTheLengthAndThePosition_WithADotWhateverTheCulture(string culture)
    {
        string html = await RenderAsync(12.5, 215.3, culture);

        Assert.Contains("max=\"215.3\"", html);
        Assert.Contains("value=\"12.5\"", html);
        Assert.Contains("step=\"0.01\"", html);
        Assert.Contains("--seek-progress: 5.81%", html);
    }
}
