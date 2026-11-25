using System.ComponentModel;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Features.MetadataList;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class FeatureMetadataListStateExtension
{
    extension(FeatureMetadataListState state)
    {
        // The items of a kind in name order, ignoring case.
        public List<MetadataItem> ItemListOf(MetadataKindEnum kind)
        {
            List<MetadataItem> itemList = kind switch
            {
                MetadataKindEnum.Artist => state.ArtistList.Select(x => new MetadataItem(x.ID, x.Name, x.MediaFileCount)).ToList(),
                MetadataKindEnum.Album => state.AlbumList.Select(x => new MetadataItem(x.ID, x.Title, x.MediaFileCount)).ToList(),
                MetadataKindEnum.Genre => state.GenreList.Select(x => new MetadataItem(x.ID, x.Name, x.MediaFileCount)).ToList(),
                _ => throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(MetadataKindEnum))
            };

            return itemList.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }

    extension(List<MetadataItem> itemList)
    {
        // The items whose name contains the text, ignoring case and the spaces around the text; a blank text keeps all. Always a new list.
        public List<MetadataItem> FilterByName(string? text)
        {
            string trimmed = text?.Trim() ?? string.Empty;

            return trimmed.Length == 0
                ? [.. itemList]
                : itemList.Where(x => x.Name.Contains(trimmed, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}
