using System.ComponentModel.DataAnnotations;
using Noxy.NET.UI.Components.InputGroups;
using Noxy.NET.UI.Models.Forms;

namespace Noxy.NET.Test.Shared;

public class InputGroupCompositeTest
{
    private sealed class AddressModel
    {
        [Required]
        public string? Scheme { get; set; }

        [Required]
        public string? Host { get; set; }

        public string? Note { get; set; }
    }

    private static WebFormContext<AddressModel> InvalidContext()
    {
        WebFormContext<AddressModel> context = new(new());
        context.Validate();
        return context;
    }

    [Fact]
    public void CombineErrorList_GathersTheErrorsOfEveryNamedField_InTheOrderOfTheNames()
    {
        WebFormContext<AddressModel> context = InvalidContext();

        List<string> list = InputGroupComposite.CombineErrorList(context, ["Host", "Scheme"], null);

        Assert.Equal([.. context.GetFieldErrorList("Host"), .. context.GetFieldErrorList("Scheme")], list);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void CombineErrorList_AddsTheErrorsTheCallerGave_AfterTheFieldErrors()
    {
        WebFormContext<AddressModel> context = InvalidContext();

        List<string> list = InputGroupComposite.CombineErrorList(context, ["Scheme"], ["The address cannot be reached."]);

        Assert.Equal([.. context.GetFieldErrorList("Scheme"), "The address cannot be reached."], list);
    }

    [Fact]
    public void CombineErrorList_SkipsFieldsWithoutErrors_UnknownNames_AndRepeats()
    {
        WebFormContext<AddressModel> context = InvalidContext();

        List<string> list = InputGroupComposite.CombineErrorList(context, ["Note", "Missing", "Scheme", "Scheme"], [.. context.GetFieldErrorList("Scheme")]);

        Assert.Equal(context.GetFieldErrorList("Scheme"), list);
    }

    [Fact]
    public void CombineErrorList_WithoutAContextOrNames_ReturnsOnlyTheGivenErrors()
    {
        Assert.Equal(["One", "Two"], InputGroupComposite.CombineErrorList(null, ["Scheme"], ["One", "Two"]));
        Assert.Equal(["One"], InputGroupComposite.CombineErrorList(InvalidContext(), null, ["One"]));
        Assert.Empty(InputGroupComposite.CombineErrorList(null, null, null));
    }
}
