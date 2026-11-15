using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Requests.Metadata;

public class RequestMergeItem : IValidatableObject
{
    public Guid TargetID { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        return TargetID == Guid.Empty ? [new($"The field {nameof(TargetID)} is required.", [nameof(TargetID)])] : [];
    }

    public CommandArtistMerge ToArtistCommand(Guid userID, Guid id)
    {
        return new(userID, id, TargetID);
    }

    public CommandGenreMerge ToGenreCommand(Guid userID, Guid id)
    {
        return new(userID, id, TargetID);
    }

    public CommandAlbumMerge ToAlbumCommand(Guid userID, Guid id)
    {
        return new(userID, id, TargetID);
    }
}
