using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class MetadataSetModelTest
{
    private static readonly Guid ArtistA = Guid.CreateVersion7();
    private static readonly Guid ArtistB = Guid.CreateVersion7();
    private static readonly Guid AlbumX = Guid.CreateVersion7();
    private static readonly Guid AlbumY = Guid.CreateVersion7();

    private static ResponseMediaFile WithArtist(params Guid[] artistIDList) => MediaFileFactory.Create(artistIDList: [.. artistIDList]);

    private static ResponseMediaFile WithAlbum(Guid? albumID) => MediaFileFactory.Create(albumID: albumID);

    [Fact]
    public void StateOf_IsCheckedWhenEveryMediaFileHasTheItem_MixedWhenSomeDo_AndUncheckedWhenNoneDo()
    {
        MetadataSetModel model = new(MetadataKindEnum.Artist, [WithArtist(ArtistA, ArtistB), WithArtist(ArtistA), WithArtist()]);

        Assert.Equal(MetadataCheckStateEnum.Mixed, model.StateOf(ArtistA));
        Assert.Equal(MetadataCheckStateEnum.Mixed, model.StateOf(ArtistB));
        Assert.Equal(MetadataCheckStateEnum.Unchecked, model.StateOf(Guid.CreateVersion7()));
        Assert.Equal(MetadataCheckStateEnum.Checked, new MetadataSetModel(MetadataKindEnum.Artist, [WithArtist(ArtistA), WithArtist(ArtistA)]).StateOf(ArtistA));
    }

    [Fact]
    public void Toggle_Unchecked_ChecksIt_AndSavingAddsItToEveryMediaFile()
    {
        List<ResponseMediaFile> list = [WithArtist(), WithArtist()];
        MetadataSetModel model = new(MetadataKindEnum.Artist, list);

        model.Toggle(ArtistA);

        Assert.Equal(MetadataCheckStateEnum.Checked, model.StateOf(ArtistA));
        MetadataChange change = Assert.Single(model.GetChangeList());
        Assert.True(change.IsAdd);
        Assert.Equal(ArtistA, change.ItemID);
        Assert.Equal(list.Select(x => x.ID), change.MediaFileIDList);
    }

    [Fact]
    public void Toggle_Mixed_ChecksIt_AndSavingAddsItOnlyToTheMediaFilesWithoutIt()
    {
        ResponseMediaFile has = WithArtist(ArtistA);
        ResponseMediaFile lacks = WithArtist();
        MetadataSetModel model = new(MetadataKindEnum.Artist, [has, lacks]);

        model.Toggle(ArtistA);

        MetadataChange change = Assert.Single(model.GetChangeList());
        Assert.True(change.IsAdd);
        Assert.Equal([lacks.ID], change.MediaFileIDList);
    }

    [Fact]
    public void Toggle_Checked_UnchecksIt_AndSavingRemovesItFromTheMediaFilesThatHaveIt()
    {
        ResponseMediaFile first = WithArtist(ArtistA);
        ResponseMediaFile second = WithArtist(ArtistA);
        MetadataSetModel model = new(MetadataKindEnum.Artist, [first, second]);

        model.Toggle(ArtistA);

        MetadataChange change = Assert.Single(model.GetChangeList());
        Assert.False(change.IsAdd);
        Assert.Equal([first.ID, second.ID], change.MediaFileIDList);
    }

    [Fact]
    public void Toggle_MixedThreeTimes_IsBackToMixed_WithNothingToSave()
    {
        MetadataSetModel model = new(MetadataKindEnum.Artist, [WithArtist(ArtistA), WithArtist()]);

        model.Toggle(ArtistA);
        model.Toggle(ArtistA);
        model.Toggle(ArtistA);

        Assert.Equal(MetadataCheckStateEnum.Mixed, model.StateOf(ArtistA));
        Assert.Empty(model.GetChangeList());
        Assert.False(model.HasChange);
    }

    [Fact]
    public void Toggle_CheckedTwice_IsBackToChecked_WithNothingToSave()
    {
        MetadataSetModel model = new(MetadataKindEnum.Genre, [MediaFileFactory.Create(genreIDList: [ArtistA])]);

        model.Toggle(ArtistA);
        model.Toggle(ArtistA);

        Assert.Empty(model.GetChangeList());
    }

    [Fact]
    public void GetChangeList_HasOneChangePerItemThatWasTouched()
    {
        MetadataSetModel model = new(MetadataKindEnum.Artist, [WithArtist(ArtistA), WithArtist()]);

        model.Toggle(ArtistA);
        model.Toggle(ArtistB);

        Assert.Equal(2, model.GetChangeList().Count);
        Assert.True(model.HasChange);
    }

    [Fact]
    public void Album_Choose_SavingSetsItOnTheMediaFilesThatHaveAnotherAlbum_OrNone()
    {
        ResponseMediaFile none = WithAlbum(null);
        ResponseMediaFile other = WithAlbum(AlbumY);
        ResponseMediaFile already = WithAlbum(AlbumX);
        MetadataSetModel model = new(MetadataKindEnum.Album, [none, other, already]);

        model.Choose(AlbumX);

        Assert.Equal(MetadataCheckStateEnum.Checked, model.StateOf(AlbumX));
        Assert.Equal(MetadataCheckStateEnum.Unchecked, model.StateOf(AlbumY));
        MetadataChange change = Assert.Single(model.GetChangeList());
        Assert.True(change.IsAdd);
        Assert.Equal(AlbumX, change.ItemID);
        Assert.Equal([none.ID, other.ID], change.MediaFileIDList);
    }

    [Fact]
    public void Album_ChooseNone_SavingRemovesEachAlbumFromItsMediaFiles()
    {
        ResponseMediaFile onX1 = WithAlbum(AlbumX);
        ResponseMediaFile onX2 = WithAlbum(AlbumX);
        ResponseMediaFile onY = WithAlbum(AlbumY);
        ResponseMediaFile none = WithAlbum(null);
        MetadataSetModel model = new(MetadataKindEnum.Album, [onX1, onX2, onY, none]);

        model.Choose(null);

        Assert.True(model.IsNoAlbumChosen);
        List<MetadataChange> list = model.GetChangeList();
        Assert.Equal(2, list.Count);
        Assert.All(list, x => Assert.False(x.IsAdd));
        Assert.Equal([onX1.ID, onX2.ID], list.Single(x => x.ItemID == AlbumX).MediaFileIDList);
        Assert.Equal([onY.ID], list.Single(x => x.ItemID == AlbumY).MediaFileIDList);
    }

    [Fact]
    public void Album_WithNothingChosen_HasNothingToSave_AndNoAlbumIsChosenOnlyWhenNoMediaFileHasOne()
    {
        Assert.Empty(new MetadataSetModel(MetadataKindEnum.Album, [WithAlbum(AlbumX), WithAlbum(null)]).GetChangeList());
        Assert.True(new MetadataSetModel(MetadataKindEnum.Album, [WithAlbum(null)]).IsNoAlbumChosen);
        Assert.False(new MetadataSetModel(MetadataKindEnum.Album, [WithAlbum(AlbumX), WithAlbum(null)]).IsNoAlbumChosen);
    }

    [Fact]
    public void Album_ChoosingTheAlbumEveryMediaFileHasAlready_HasNothingToSave()
    {
        MetadataSetModel model = new(MetadataKindEnum.Album, [WithAlbum(AlbumX), WithAlbum(AlbumX)]);

        model.Choose(AlbumX);

        Assert.Empty(model.GetChangeList());
    }
}
