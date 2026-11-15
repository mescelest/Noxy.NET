using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Requests.Metadata;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class RequestMediaFileListTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly Guid ID = Guid.CreateVersion7();

    private static List<ValidationResult> Validate(object request)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(request, new(request), results, true);
        return results;
    }

    private static List<Guid> IDList(int count)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Guid.CreateVersion7())];
    }

    [Fact]
    public void Add_OneToTheMaximum_IsValid()
    {
        Assert.Empty(Validate(new RequestAddMediaFileList { MediaFileIDList = IDList(1) }));
        Assert.Empty(Validate(new RequestAddMediaFileList { MediaFileIDList = IDList(RequestAddMediaFileList.MaxMediaFileCount) }));
    }

    [Fact]
    public void Add_EmptyOrTooMany_IsInvalid()
    {
        Assert.Contains("MediaFileIDList", Assert.Single(Validate(new RequestAddMediaFileList())).MemberNames);
        Assert.Single(Validate(new RequestAddMediaFileList { MediaFileIDList = IDList(RequestAddMediaFileList.MaxMediaFileCount + 1) }));
    }

    [Fact]
    public void Remove_OneToTheMaximum_IsValid_AndEmptyOrTooManyIsInvalid()
    {
        Assert.Empty(Validate(new RequestRemoveMediaFileList { MediaFileIDList = IDList(3) }));
        Assert.Single(Validate(new RequestRemoveMediaFileList()));
        Assert.Single(Validate(new RequestRemoveMediaFileList { MediaFileIDList = IDList(RequestAddMediaFileList.MaxMediaFileCount + 1) }));
    }

    [Fact]
    public void Add_ToCommand_CarriesTheUserTheIdAndTheFiles_ForEveryKind()
    {
        List<Guid> idList = IDList(2);
        RequestAddMediaFileList request = new() { MediaFileIDList = idList };

        Assert.Equal((UserID, ID, idList), (request.ToArtistCommand(UserID, ID).UserID, request.ToArtistCommand(UserID, ID).ID, request.ToArtistCommand(UserID, ID).MediaFileIDList));
        Assert.Equal(idList, request.ToGenreCommand(UserID, ID).MediaFileIDList);
        Assert.Equal(idList, request.ToAlbumCommand(UserID, ID).MediaFileIDList);
        Assert.Equal(ID, request.ToGenreCommand(UserID, ID).ID);
        Assert.Equal(UserID, request.ToAlbumCommand(UserID, ID).UserID);
    }

    [Fact]
    public void Remove_ToCommand_CarriesTheUserTheIdAndTheFiles_ForEveryKind()
    {
        List<Guid> idList = IDList(2);
        RequestRemoveMediaFileList request = new() { MediaFileIDList = idList };

        Assert.Equal(idList, request.ToArtistCommand(UserID, ID).MediaFileIDList);
        Assert.Equal(idList, request.ToGenreCommand(UserID, ID).MediaFileIDList);
        Assert.Equal(idList, request.ToAlbumCommand(UserID, ID).MediaFileIDList);
        Assert.Equal(ID, request.ToArtistCommand(UserID, ID).ID);
        Assert.Equal(UserID, request.ToGenreCommand(UserID, ID).UserID);
    }
}
