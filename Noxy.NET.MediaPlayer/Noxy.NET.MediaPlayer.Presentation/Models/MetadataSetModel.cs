using System.ComponentModel;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

// What the "Set artist / album / genre" dialog edits: for the media files it was opened on, which items they carry. The user changes the picture and the changes are worked out against what the media files have; nothing is sent from here.
public class MetadataSetModel
{
    private readonly MetadataKindEnum _kind;
    private readonly List<ResponseMediaFile> _mediaFileList;

    // The state the user moved an item to; an item that was put back where it started is not in it.
    private readonly Dictionary<Guid, MetadataCheckStateEnum> _changedMap = [];
    private bool _isAlbumChosen;
    private Guid? _chosenAlbumID;

    public MetadataSetModel(MetadataKindEnum kind, List<ResponseMediaFile> mediaFileList)
    {
        _kind = kind;
        _mediaFileList = mediaFileList;
    }

    public bool HasChange => GetChangeList().Count > 0;

    // True when no album is the choice: nothing was changed and no media file has one, or the user chose "none".
    public bool IsNoAlbumChosen => _isAlbumChosen ? _chosenAlbumID is null : _mediaFileList.All(x => x.AlbumID is null);

    public MetadataCheckStateEnum StateOf(Guid itemID)
    {
        if (_kind == MetadataKindEnum.Album) return StateOfAlbum(itemID);

        return _changedMap.TryGetValue(itemID, out MetadataCheckStateEnum changed) ? changed : OriginalStateOf(itemID);
    }

    // Unchecked becomes checked, checked becomes unchecked, and mixed becomes checked; going round again returns a mixed item to mixed.
    public void Toggle(Guid itemID)
    {
        MetadataCheckStateEnum original = OriginalStateOf(itemID);
        MetadataCheckStateEnum next = StateOf(itemID) switch
        {
            MetadataCheckStateEnum.Unchecked => original == MetadataCheckStateEnum.Mixed ? MetadataCheckStateEnum.Mixed : MetadataCheckStateEnum.Checked,
            MetadataCheckStateEnum.Checked => MetadataCheckStateEnum.Unchecked,
            MetadataCheckStateEnum.Mixed => MetadataCheckStateEnum.Checked,
            _ => throw new InvalidEnumArgumentException(nameof(itemID), (int)StateOf(itemID), typeof(MetadataCheckStateEnum))
        };

        if (next == original) _changedMap.Remove(itemID);
        else _changedMap[itemID] = next;
    }

    // For an album: the one album the media files get, or none.
    public void Choose(Guid? albumID)
    {
        _isAlbumChosen = true;
        _chosenAlbumID = albumID;
    }

    // The requests that bring the media files to what the dialog shows. An item that is already as wanted on a media file is not sent for it.
    public List<MetadataChange> GetChangeList()
    {
        return _kind == MetadataKindEnum.Album ? GetAlbumChangeList() : GetItemChangeList();
    }

    private List<MetadataChange> GetItemChangeList()
    {
        List<MetadataChange> list = [];

        foreach ((Guid itemID, MetadataCheckStateEnum state) in _changedMap)
        {
            if (state == MetadataCheckStateEnum.Checked)
            {
                List<Guid> lackList = [.. _mediaFileList.Where(x => !Carries(x, itemID)).Select(x => x.ID)];
                if (lackList.Count > 0) list.Add(new(itemID, true, lackList));
            }
            else if (state == MetadataCheckStateEnum.Unchecked)
            {
                List<Guid> haveList = [.. _mediaFileList.Where(x => Carries(x, itemID)).Select(x => x.ID)];
                if (haveList.Count > 0) list.Add(new(itemID, false, haveList));
            }
        }

        return list;
    }

    private List<MetadataChange> GetAlbumChangeList()
    {
        if (!_isAlbumChosen) return [];

        if (_chosenAlbumID is { } chosen)
        {
            List<Guid> otherList = [.. _mediaFileList.Where(x => x.AlbumID != chosen).Select(x => x.ID)];

            return otherList.Count > 0 ? [new(chosen, true, otherList)] : [];
        }

        return [.. _mediaFileList.Where(x => x.AlbumID is not null).GroupBy(x => x.AlbumID.GetValueOrDefault()).Select(group => new MetadataChange(group.Key, false, [.. group.Select(x => x.ID)]))];
    }

    private MetadataCheckStateEnum StateOfAlbum(Guid albumID)
    {
        if (_isAlbumChosen) return _chosenAlbumID == albumID ? MetadataCheckStateEnum.Checked : MetadataCheckStateEnum.Unchecked;

        return OriginalStateOf(albumID);
    }

    private MetadataCheckStateEnum OriginalStateOf(Guid itemID)
    {
        int count = _mediaFileList.Count(x => Carries(x, itemID));
        if (count == 0) return MetadataCheckStateEnum.Unchecked;

        return count == _mediaFileList.Count ? MetadataCheckStateEnum.Checked : MetadataCheckStateEnum.Mixed;
    }

    private bool Carries(ResponseMediaFile mediaFile, Guid itemID)
    {
        return _kind switch
        {
            MetadataKindEnum.Artist => mediaFile.ArtistIDList.Contains(itemID),
            MetadataKindEnum.Album => mediaFile.AlbumID == itemID,
            MetadataKindEnum.Genre => mediaFile.GenreIDList.Contains(itemID),
            _ => throw new InvalidEnumArgumentException(nameof(_kind), (int)_kind, typeof(MetadataKindEnum))
        };
    }
}
