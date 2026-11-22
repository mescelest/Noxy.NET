using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Requests.Metadata;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class RequestMergeAndSplitItemTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly Guid ID = Guid.CreateVersion7();

    private static List<ValidationResult> Validate(object request)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(request, new(request), results, true);
        return results;
    }

    [Fact]
    public void Merge_ATarget_IsValid_AndNoTargetIsInvalid()
    {
        Assert.Empty(Validate(new RequestMergeItem { TargetID = Guid.CreateVersion7() }));
        Assert.Contains("TargetID", Assert.Single(Validate(new RequestMergeItem())).MemberNames);
    }

    [Fact]
    public void Merge_ToCommand_CarriesTheUserTheIdAndTheTarget_ForEveryKind()
    {
        Guid targetID = Guid.CreateVersion7();
        RequestMergeItem request = new() { TargetID = targetID };

        Assert.Equal(targetID, request.ToArtistCommand(UserID, ID).TargetID);
        Assert.Equal(targetID, request.ToGenreCommand(UserID, ID).TargetID);
        Assert.Equal(targetID, request.ToAlbumCommand(UserID, ID).TargetID);
        Assert.Equal(ID, request.ToArtistCommand(UserID, ID).ID);
        Assert.Equal(UserID, request.ToAlbumCommand(UserID, ID).UserID);
    }

    [Fact]
    public void Split_TwoToTheMaximumNames_IsValid()
    {
        Assert.Empty(Validate(new RequestSplitItem { NameList = ["A", "B"] }));
        Assert.Empty(Validate(new RequestSplitItem { NameList = [.. Enumerable.Range(0, RequestSplitItem.MaxNameCount).Select(x => $"Name {x}")] }));
    }

    [Theory]
    [InlineData]
    [InlineData("A")]
    public void Split_FewerThanTwoNames_IsInvalid(params string[] nameList)
    {
        Assert.Contains("NameList", Assert.Single(Validate(new RequestSplitItem { NameList = [.. nameList] })).MemberNames);
    }

    [Fact]
    public void Split_TooManyNames_IsInvalid()
    {
        Assert.Single(Validate(new RequestSplitItem { NameList = [.. Enumerable.Range(0, RequestSplitItem.MaxNameCount + 1).Select(x => $"Name {x}")] }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Split_ABlankName_IsInvalid(string blank)
    {
        Assert.Contains("NameList", Assert.Single(Validate(new RequestSplitItem { NameList = ["A", blank] })).MemberNames);
    }

    [Fact]
    public void Split_ANameOver255Characters_IsInvalid()
    {
        Assert.Single(Validate(new RequestSplitItem { NameList = ["A", new('b', RequestSplitItem.MaxNameLength + 1)] }));
    }

    [Fact]
    public void Split_ToCommand_CarriesTheUserTheIdAndTheNames_ForArtistAndGenre()
    {
        RequestSplitItem request = new() { NameList = ["A", "B"] };

        Assert.Equal(["A", "B"], request.ToArtistCommand(UserID, ID).NameList);
        Assert.Equal(["A", "B"], request.ToGenreCommand(UserID, ID).NameList);
        Assert.Equal(ID, request.ToArtistCommand(UserID, ID).ID);
        Assert.Equal(UserID, request.ToGenreCommand(UserID, ID).UserID);
    }
}
