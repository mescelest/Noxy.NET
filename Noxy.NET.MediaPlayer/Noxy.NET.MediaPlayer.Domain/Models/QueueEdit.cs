using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.Domain.Models;

public class QueueEdit : IValidatableObject
{
    public required QueueEditEnum Edit { get; init; }
    public Guid? ItemID { get; init; }
    public Guid? MediaFileID { get; init; }
    public Guid? AfterItemID { get; init; }
    public QueueItem? Item { get; init; }
    public List<QueueItem?>? ItemList { get; init; }
    public ResponseQueueSource? Source { get; init; }
    public Guid? CurrentItemID { get; init; }
    public bool KeepUserQueue { get; init; }
    public bool KeepSkipped { get; init; }

    // Each edit needs the fields it works on; fields it does not use are ignored.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        List<ValidationResult> errorList = [];
        switch (Edit)
        {
            case QueueEditEnum.PlayNext:
            case QueueEditEnum.AddToQueue:
                Require(errorList, ItemID is { } newItemID && newItemID != Guid.Empty, nameof(ItemID));
                Require(errorList, MediaFileID is { } mediaFileID && mediaFileID != Guid.Empty, nameof(MediaFileID));
                break;
            case QueueEditEnum.Remove:
            case QueueEditEnum.Move:
            case QueueEditEnum.RemoveSourceItem:
                Require(errorList, ItemID is { } itemID && itemID != Guid.Empty, nameof(ItemID));
                break;
            case QueueEditEnum.ReplaceSource:
                Require(errorList, Source is not null, nameof(Source));
                Require(errorList, ItemList is not null, nameof(ItemList));
                break;
            case QueueEditEnum.RegenerateSource:
                Require(errorList, ItemList is not null, nameof(ItemList));
                break;
            case QueueEditEnum.InsertSourceItem:
            case QueueEditEnum.Previous:
                Require(errorList, Item is not null, nameof(Item));
                break;
            case QueueEditEnum.Advance:
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(Edit), (int)Edit, typeof(QueueEditEnum));
        }

        Require(errorList, Item is null || (Item.ItemID != Guid.Empty && Item.MediaFileID != Guid.Empty), nameof(Item));
        Require(errorList, ItemList is null || ItemList.All(x => x is { } item && item.ItemID != Guid.Empty && item.MediaFileID != Guid.Empty), nameof(ItemList));
        return errorList;
    }

    private static void Require(List<ValidationResult> errorList, bool valid, string member)
    {
        if (!valid) errorList.Add(new($"The field {member} is required or not valid for this edit.", [member]));
    }
}
