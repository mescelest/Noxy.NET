using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.Test.MediaPlayer.Remote;

public class QueueEditTest
{
    private static readonly Guid ItemID = Guid.CreateVersion7();
    private static readonly Guid MediaFileID = Guid.CreateVersion7();
    private static readonly QueueItem Item = new(Guid.CreateVersion7(), Guid.CreateVersion7());
    private static readonly ResponseQueueSource Source = new() { Kind = QueueSourceKindEnum.Library, PlaylistID = null, Sort = LibrarySortEnum.Title, Descending = false, Search = string.Empty };

    private static List<ValidationResult> Validate(QueueEdit edit)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(edit, new(edit), results, true);
        return results;
    }

    [Fact]
    public void PlayNext_WithAnItemIDAndAMediaFileID_IsValid()
    {
        Assert.Empty(Validate(new() { Edit = QueueEditEnum.PlayNext, ItemID = ItemID, MediaFileID = MediaFileID }));
    }

    [Fact]
    public void PlayNext_WithoutAMediaFileID_IsInvalid()
    {
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.PlayNext, ItemID = ItemID }));
    }

    [Fact]
    public void PlayNext_WithoutAnItemID_IsInvalid()
    {
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.PlayNext, MediaFileID = MediaFileID }));
    }

    [Fact]
    public void AddToQueue_RequiresAnItemIDAndAMediaFileID()
    {
        Assert.Empty(Validate(new() { Edit = QueueEditEnum.AddToQueue, ItemID = ItemID, MediaFileID = MediaFileID }));
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.AddToQueue, ItemID = ItemID }));
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.AddToQueue, MediaFileID = MediaFileID }));
    }

    [Fact]
    public void Remove_RequiresAnItemID()
    {
        Assert.Empty(Validate(new() { Edit = QueueEditEnum.Remove, ItemID = ItemID }));
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.Remove }));
    }

    [Fact]
    public void Move_RequiresAnItemID_AndTheAnchorIsOptional()
    {
        Assert.Empty(Validate(new() { Edit = QueueEditEnum.Move, ItemID = ItemID }));
        Assert.Empty(Validate(new() { Edit = QueueEditEnum.Move, ItemID = ItemID, AfterItemID = Guid.CreateVersion7() }));
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.Move, AfterItemID = Guid.CreateVersion7() }));
    }

    [Fact]
    public void ReplaceSource_WithoutASourceOrAnItemList_IsInvalid()
    {
        Assert.Empty(Validate(new() { Edit = QueueEditEnum.ReplaceSource, Source = Source, ItemList = [Item], CurrentItemID = Item.ItemID }));
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.ReplaceSource, ItemList = [Item] }));
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.ReplaceSource, Source = Source }));
    }

    [Fact]
    public void ReplaceSource_WithAnEmptyItemListAndNoCurrentItem_IsValid()
    {
        Assert.Empty(Validate(new() { Edit = QueueEditEnum.ReplaceSource, Source = Source, ItemList = [] }));
    }

    [Fact]
    public void RegenerateSource_RequiresAnItemList()
    {
        Assert.Empty(Validate(new() { Edit = QueueEditEnum.RegenerateSource, ItemList = [Item] }));
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.RegenerateSource }));
    }

    [Fact]
    public void InsertSourceItem_RequiresAnItem_AndTheAnchorIsOptional()
    {
        Assert.Empty(Validate(new() { Edit = QueueEditEnum.InsertSourceItem, Item = Item }));
        Assert.Empty(Validate(new() { Edit = QueueEditEnum.InsertSourceItem, Item = Item, AfterItemID = Guid.CreateVersion7() }));
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.InsertSourceItem, AfterItemID = Guid.CreateVersion7() }));
    }

    [Fact]
    public void RemoveSourceItem_RequiresAnItemID()
    {
        Assert.Empty(Validate(new() { Edit = QueueEditEnum.RemoveSourceItem, ItemID = ItemID }));
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.RemoveSourceItem }));
    }

    [Fact]
    public void Advance_WithNoItem_IsValid()
    {
        Assert.Empty(Validate(new() { Edit = QueueEditEnum.Advance }));
        Assert.Empty(Validate(new() { Edit = QueueEditEnum.Advance, Item = Item, KeepSkipped = true }));
    }

    [Fact]
    public void Previous_WithoutAnItem_IsInvalid()
    {
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.Previous }));
        Assert.Empty(Validate(new() { Edit = QueueEditEnum.Previous, Item = Item }));
    }

    [Fact]
    public void AnItemWithAnEmptyID_IsInvalid()
    {
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.Advance, Item = new(Guid.Empty, Guid.CreateVersion7()) }));
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.Remove, ItemID = Guid.Empty }));
    }

    [Fact]
    public void AnEditIsReadFromJsonByName_AndAnUnknownNameFailsToBind()
    {
        QueueEdit edit = JsonSerializer.Deserialize<QueueEdit>($$"""{"edit":"PlayNext","itemID":"{{ItemID}}","mediaFileID":"{{MediaFileID}}"}""", JsonSerializerOptions.Web) ?? throw new InvalidOperationException("No edit.");

        Assert.Equal(QueueEditEnum.PlayNext, edit.Edit);
        Assert.Equal(ItemID, edit.ItemID);
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<QueueEdit>("""{"edit":"ANameThatIsNotAnEdit"}""", JsonSerializerOptions.Web));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<QueueEdit>("""{"edit":0}""", JsonSerializerOptions.Web));
    }

    [Fact]
    public void TheResponsesSerialiseEnumsByNameInCamelCase()
    {
        string source = JsonSerializer.Serialize(Source, JsonSerializerOptions.Web);
        string edit = JsonSerializer.Serialize(new QueueEdit { Edit = QueueEditEnum.RemoveSourceItem, ItemID = ItemID }, JsonSerializerOptions.Web);
        string delta = JsonSerializer.Serialize(new ResponseQueueDelta { QueueRevision = 3, Edit = QueueEditEnum.Advance }, JsonSerializerOptions.Web);

        Assert.Contains("\"kind\":\"Library\"", source);
        Assert.Contains("\"sort\":\"Title\"", source);
        Assert.Contains("\"edit\":\"RemoveSourceItem\"", edit);
        Assert.Contains("\"edit\":\"Advance\"", delta);
        Assert.Contains("\"queueRevision\":3", delta);
    }

    [Fact]
    public void ThePlayerStateSerialisesItsModesByName()
    {
        ResponsePlayerState state = new()
        {
            ActiveDeviceID = null,
            StateRevision = 1,
            IsPlaying = true,
            PositionSeconds = 1.5,
            ServerTime = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Volume = 40,
            Shuffle = ShuffleModeEnum.SpreadArtists,
            Repeat = RepeatModeEnum.Forever,
            CurrentItemID = null,
            Title = null,
            Artist = null
        };

        string json = JsonSerializer.Serialize(state, JsonSerializerOptions.Web);

        Assert.Contains("\"shuffle\":\"SpreadArtists\"", json);
        Assert.Contains("\"repeat\":\"Forever\"", json);
    }

    [Fact]
    public void AnUndefinedEdit_ThrowsAnInvalidEnumException_WhenValidated()
    {
        Assert.Throws<InvalidEnumArgumentException>(() => Validate(new() { Edit = (QueueEditEnum)999 }));
    }

    [Fact]
    public void AnItemListWithANullItem_IsInvalid_InsteadOfFailingWithANullReference()
    {
        Assert.NotEmpty(Validate(new() { Edit = QueueEditEnum.RegenerateSource, ItemList = [Item, null!] }));
    }
}
