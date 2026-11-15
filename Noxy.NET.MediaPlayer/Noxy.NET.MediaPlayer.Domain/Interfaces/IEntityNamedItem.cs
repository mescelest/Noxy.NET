namespace Noxy.NET.MediaPlayer.Domain.Interfaces;

public interface IEntityNamedItem
{
    Guid ID { get; }
    Guid UserID { get; }
    string Name { get; set; }
    string NameKey { get; set; }
    DateTime TimeModified { get; set; }
}
