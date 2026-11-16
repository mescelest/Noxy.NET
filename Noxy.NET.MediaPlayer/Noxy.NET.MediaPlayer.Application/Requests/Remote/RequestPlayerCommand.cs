using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Application.Requests.Remote;

public class RequestPlayerCommand : IValidatableObject
{
    public required PlayerCommandEnum Command { get; init; }
    public double? PositionSeconds { get; init; }
    public int? Volume { get; init; }
    public ShuffleModeEnum? Shuffle { get; init; }
    public RepeatModeEnum? Repeat { get; init; }
    public Guid? MediaFileID { get; init; }
    public Guid? PlaylistID { get; init; }
    public Guid? EntryID { get; init; }
    public Guid? ItemID { get; init; }
    public bool? KeepSkipped { get; init; }
    public LibrarySortEnum? Sort { get; init; }
    public bool? Descending { get; init; }

    [MaxLength(500)]
    public string? Search { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        List<ValidationResult> errorList = [];
        switch (Command)
        {
            case PlayerCommandEnum.Seek when PositionSeconds is not >= 0:
                errorList.Add(new("A seek needs a position of 0 or more.", [nameof(PositionSeconds)]));
                break;
            case PlayerCommandEnum.JumpTo when ItemID is null || ItemID == Guid.Empty:
                errorList.Add(new("A jump needs an item.", [nameof(ItemID)]));
                break;
            case PlayerCommandEnum.SetVolume when Volume is not (>= 0 and <= 100):
                errorList.Add(new("A volume is 0 to 100.", [nameof(Volume)]));
                break;
            case PlayerCommandEnum.SetShuffle when Shuffle is null:
                errorList.Add(new("A shuffle change needs a mode.", [nameof(Shuffle)]));
                break;
            case PlayerCommandEnum.SetRepeat when Repeat is null:
                errorList.Add(new("A repeat change needs a mode.", [nameof(Repeat)]));
                break;
            case PlayerCommandEnum.PlayLibrary when MediaFileID is null || MediaFileID == Guid.Empty:
                errorList.Add(new("Playing the library needs a media file.", [nameof(MediaFileID)]));
                break;
            case PlayerCommandEnum.PlayLibrary when Sort is null:
                errorList.Add(new("Playing the library needs a sort.", [nameof(Sort)]));
                break;
            case PlayerCommandEnum.PlayPlaylist when PlaylistID is null || PlaylistID == Guid.Empty:
                errorList.Add(new("Playing a playlist needs a playlist.", [nameof(PlaylistID)]));
                break;
            case PlayerCommandEnum.Play:
            case PlayerCommandEnum.Pause:
            case PlayerCommandEnum.Next:
            case PlayerCommandEnum.Previous:
            case PlayerCommandEnum.Seek:
            case PlayerCommandEnum.JumpTo:
            case PlayerCommandEnum.SetVolume:
            case PlayerCommandEnum.SetShuffle:
            case PlayerCommandEnum.SetRepeat:
            case PlayerCommandEnum.PlayLibrary:
            case PlayerCommandEnum.PlayPlaylist:
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(Command), (int)Command, typeof(PlayerCommandEnum));
        }

        return errorList;
    }
}
