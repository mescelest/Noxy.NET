using Noxy.NET.UI.Models;

namespace Noxy.NET.Test.Shared;

public class ContextMenuNavigatorTest
{
    [Fact]
    public void MoveNext_SkipsSeparatorsAndDisabledItems()
    {
        ContextMenuNavigator navigator = new([ContextMenuItem.Item("Alpha"), ContextMenuItem.Separator(), ContextMenuItem.Item("Beta", isDisabled: true), ContextMenuItem.Item("Gamma")], false);

        navigator.MoveNext();
        Assert.Equal(0, navigator.FocusIndex);

        navigator.MoveNext();
        Assert.Equal(3, navigator.FocusIndex);
    }

    [Fact]
    public void MoveNext_WrapsAtTheEnd()
    {
        ContextMenuNavigator navigator = new([ContextMenuItem.Item("Alpha"), ContextMenuItem.Item("Beta")], false);

        navigator.MoveNext();
        navigator.MoveNext();
        navigator.MoveNext();

        Assert.Equal(0, navigator.FocusIndex);
    }

    [Fact]
    public void MovePrevious_StartsAtTheLastEnabledItem_AndWrapsAtTheStart()
    {
        ContextMenuNavigator navigator = new([ContextMenuItem.Item("Alpha"), ContextMenuItem.Item("Beta"), ContextMenuItem.Separator()], false);

        navigator.MovePrevious();
        Assert.Equal(1, navigator.FocusIndex);

        navigator.MovePrevious();
        Assert.Equal(0, navigator.FocusIndex);

        navigator.MovePrevious();
        Assert.Equal(1, navigator.FocusIndex);
    }

    [Fact]
    public void MoveFirst_And_MoveLast_LandOnEnabledItems()
    {
        ContextMenuNavigator navigator = new([ContextMenuItem.Item("Alpha", isDisabled: true), ContextMenuItem.Item("Beta"), ContextMenuItem.Item("Gamma"), ContextMenuItem.Item("Delta", isDisabled: true), ContextMenuItem.Separator()], false);

        navigator.MoveFirst();
        Assert.Equal(1, navigator.FocusIndex);

        navigator.MoveLast();
        Assert.Equal(2, navigator.FocusIndex);
    }

    [Fact]
    public void MoveToLetter_JumpsToTheNextEnabledItemStartingWithIt()
    {
        ContextMenuNavigator navigator = new([ContextMenuItem.Item("Apple"), ContextMenuItem.Item("Avocado", isDisabled: true), ContextMenuItem.Item("Banana"), ContextMenuItem.Item("Apricot")], false);

        navigator.MoveToLetter('a');
        Assert.Equal(0, navigator.FocusIndex);

        navigator.MoveToLetter('A');
        Assert.Equal(3, navigator.FocusIndex);

        navigator.MoveToLetter('a');
        Assert.Equal(0, navigator.FocusIndex);

        navigator.MoveToLetter('b');
        Assert.Equal(2, navigator.FocusIndex);

        navigator.MoveToLetter('z');
        Assert.Equal(2, navigator.FocusIndex);
    }

    [Fact]
    public void Choose_ReturnsTheFocusedEnabledItem()
    {
        ContextMenuItem alpha = ContextMenuItem.Item("Alpha");
        ContextMenuNavigator navigator = new([alpha], false);

        Assert.Null(navigator.Choose());

        navigator.MoveNext();
        Assert.Same(alpha, navigator.Choose());
    }

    [Fact]
    public void Choose_OnADisabledItem_ReturnsNull()
    {
        ContextMenuNavigator navigator = new([ContextMenuItem.Item("Alpha", isDisabled: true)], false);

        navigator.MoveNext();

        Assert.Null(navigator.FocusIndex);
        Assert.Null(navigator.Choose());
    }

    [Fact]
    public void Choose_OnASubmenu_ReturnsNull()
    {
        ContextMenuNavigator navigator = new([ContextMenuItem.Submenu("More", [ContextMenuItem.Item("Inner")])], false);

        navigator.MoveNext();

        Assert.Null(navigator.Choose());
    }

    [Fact]
    public void OpenSubmenu_OnAnEmptyOrAllDisabledSubmenu_DoesNothing()
    {
        ContextMenuNavigator navigator = new([ContextMenuItem.Submenu("Empty", []), ContextMenuItem.Submenu("Disabled", [ContextMenuItem.Item("Inner", isDisabled: true), ContextMenuItem.Separator()])], false);

        navigator.MoveFirst();
        navigator.OpenSubmenu();
        Assert.Empty(navigator.OpenPath);

        navigator.MoveNext();
        navigator.OpenSubmenu();
        Assert.Empty(navigator.OpenPath);
    }

    [Fact]
    public void OpenSubmenu_OnAnItem_DoesNothing()
    {
        ContextMenuNavigator navigator = new([ContextMenuItem.Item("Alpha")], false);

        navigator.MoveNext();
        navigator.OpenSubmenu();

        Assert.Empty(navigator.OpenPath);
        Assert.Equal(0, navigator.FocusIndex);
    }

    [Fact]
    public void OpenSubmenu_ThenClose_ReturnsToTheParentWithTheSubmenuItemFocused()
    {
        ContextMenuNavigator navigator = new([ContextMenuItem.Item("Alpha"), ContextMenuItem.Submenu("More", [ContextMenuItem.Item("Inner")])], false);

        navigator.MoveNext();
        navigator.MoveNext();
        navigator.OpenSubmenu();

        Assert.Equal([1], navigator.OpenPath);
        Assert.Equal(0, navigator.FocusIndex);

        Assert.True(navigator.Close());

        Assert.Empty(navigator.OpenPath);
        Assert.Equal(1, navigator.FocusIndex);
    }

    [Fact]
    public void Close_WithNothingOpen_ReturnsFalse()
    {
        ContextMenuNavigator navigator = new([ContextMenuItem.Item("Alpha")], false);

        Assert.False(navigator.Close());
    }

    [Fact]
    public void DrillIn_ReplacesTheVisibleListAndOffersBack()
    {
        List<ContextMenuItem> inner = [ContextMenuItem.Item("Inner")];
        List<ContextMenuItem> root = [ContextMenuItem.Item("Alpha"), ContextMenuItem.Submenu("More", inner)];
        ContextMenuNavigator navigator = new(root, true);

        Assert.Same(root, navigator.VisibleList);
        Assert.False(navigator.CanGoBack);

        navigator.MoveNext();
        navigator.MoveNext();
        navigator.OpenSubmenu();

        Assert.Same(inner, navigator.VisibleList);
        Assert.True(navigator.CanGoBack);

        navigator.Close();

        Assert.Same(root, navigator.VisibleList);
        Assert.False(navigator.CanGoBack);
    }

    private static ContextMenuNavigator OpenedNavigator()
    {
        ContextMenuNavigator navigator = new([ContextMenuItem.Item("Alpha"), ContextMenuItem.Submenu("More", [ContextMenuItem.Item("X"), ContextMenuItem.Item("Y")])], false);
        navigator.MoveNext();
        navigator.MoveNext();
        navigator.OpenSubmenu();
        return navigator;
    }

    [Fact]
    public void FocusAt_FocusesTheItem_AndClosesTheLevelsBelowIt()
    {
        ContextMenuNavigator navigator = OpenedNavigator();

        navigator.FocusAt(0, 0);

        Assert.Empty(navigator.OpenPath);
        Assert.Equal(0, navigator.FocusIndex);
    }

    [Fact]
    public void FocusAt_AtTheOpenLevel_KeepsTheOpenPath()
    {
        ContextMenuNavigator navigator = OpenedNavigator();

        navigator.FocusAt(1, 1);

        Assert.Equal([1], navigator.OpenPath);
        Assert.Equal(1, navigator.FocusIndex);
    }

    [Fact]
    public void FocusAt_OnADisabledItemOrASeparator_DoesNothing()
    {
        ContextMenuNavigator navigator = new([ContextMenuItem.Item("Alpha"), ContextMenuItem.Item("Beta", isDisabled: true), ContextMenuItem.Separator()], false);
        navigator.MoveNext();

        navigator.FocusAt(0, 1);
        navigator.FocusAt(0, 2);
        navigator.FocusAt(3, 0);

        Assert.Equal(0, navigator.FocusIndex);
    }

    [Fact]
    public void CloseTo_ClosesTheLevelsFromThatDepth_AndFocusesTheItemThatOpenedThem()
    {
        ContextMenuNavigator navigator = OpenedNavigator();

        navigator.CloseTo(5);
        Assert.Equal([1], navigator.OpenPath);

        navigator.CloseTo(0);
        Assert.Empty(navigator.OpenPath);
        Assert.Equal(1, navigator.FocusIndex);
    }

    [Fact]
    public void Replace_KeepsTheOpenPathAndTheFocus_WhenTheyStillExist()
    {
        ContextMenuNavigator navigator = OpenedNavigator();
        navigator.MoveNext();

        List<ContextMenuItem> replacement = [ContextMenuItem.Item("Alpha again"), ContextMenuItem.Submenu("More", [ContextMenuItem.Item("X"), ContextMenuItem.Item("Y")])];
        navigator.Replace(replacement);

        Assert.Equal([1], navigator.OpenPath);
        Assert.Equal(1, navigator.FocusIndex);
        Assert.Same(replacement, navigator.LevelList[0]);
    }

    [Fact]
    public void Replace_DropsTheOpenPathAndTheFocus_WhenTheyNoLongerExist()
    {
        ContextMenuNavigator navigator = OpenedNavigator();

        navigator.Replace([ContextMenuItem.Item("Alpha")]);

        Assert.Empty(navigator.OpenPath);
        Assert.Null(navigator.FocusIndex);
    }

    [Fact]
    public void FlyOut_KeepsTheRootListVisibleWhileTheSubmenuIsOpen()
    {
        ContextMenuItem inner = ContextMenuItem.Item("Inner");
        List<ContextMenuItem> root = [ContextMenuItem.Submenu("More", [inner])];
        ContextMenuNavigator navigator = new(root, false);

        navigator.MoveNext();
        navigator.OpenSubmenu();

        Assert.Same(root, navigator.VisibleList);
        Assert.False(navigator.CanGoBack);
        Assert.Equal(2, navigator.LevelList.Count);
        Assert.Same(root, navigator.LevelList[0]);
        Assert.Same(inner, navigator.Choose());
    }
}
