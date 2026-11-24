using Noxy.NET.MediaPlayer.Application.Requests.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class MediaEditService(IApiClient clientApi, ISyncService serviceSync) : IMediaEditService
{
    public async Task Edit(ResponseMediaFile original, FormModelEditMedia model)
    {
        RequestUpdateMedia request = new();
        bool changed = false;

        if (Differs(original.Title, model.Title))
        {
            request.Title = new(Normalize(model.Title));
            changed = true;
        }

        if (Differs(original.Artist, model.Artist))
        {
            request.Artist = new(Normalize(model.Artist));
            changed = true;
        }

        if (Differs(original.Album, model.Album))
        {
            request.Album = new(Normalize(model.Album));
            changed = true;
        }

        if (Differs(original.AlbumArtist, model.AlbumArtist))
        {
            request.AlbumArtist = new(Normalize(model.AlbumArtist));
            changed = true;
        }

        if (Differs(original.Subtitle, model.Subtitle))
        {
            request.Subtitle = new(Normalize(model.Subtitle));
            changed = true;
        }

        if (Differs(original.Comment, model.Comment))
        {
            request.Comment = new(Normalize(model.Comment));
            changed = true;
        }

        if (Differs(original.Genre, model.Genre))
        {
            request.Genre = new(Normalize(model.Genre));
            changed = true;
        }

        if (original.TrackNumber != model.TrackNumber)
        {
            request.TrackNumber = new(model.TrackNumber);
            changed = true;
        }

        if (original.DiscNumber != model.DiscNumber)
        {
            request.DiscNumber = new(model.DiscNumber);
            changed = true;
        }

        if (original.Year != model.Year)
        {
            request.Year = new(model.Year);
            changed = true;
        }

        if (!changed) return;

        ResponseMediaFile answer = await clientApi.UpdateMedia(original.ID, request);
        await serviceSync.ApplyMediaUpdated(answer);
    }

    // Spaces around a text do not count, and no text at all is the same as none: it clears the tag.
    private static string? Normalize(string? text)
    {
        string? trimmed = text?.Trim();

        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static bool Differs(string? before, string? after)
    {
        return Normalize(before) != Normalize(after);
    }
}
