namespace Noxy.NET.MediaPlayer.Presentation.Models;

// What a right-click on a library row acts on: the clicked row, and the ticked group when the row is one of two or more ticked.
public sealed record LibraryMenuScope(Guid ClickedID, List<Guid> GroupIDList)
{
    public bool HasGroup => GroupIDList.Count > 0;

    public static LibraryMenuScope Of(Guid clickedID, IReadOnlySet<Guid> selectedSet, List<Guid> selectedIDList)
    {
        bool isInGroup = selectedSet.Count >= 2 && selectedSet.Contains(clickedID);
        return new(clickedID, isInGroup ? selectedIDList : []);
    }
}
