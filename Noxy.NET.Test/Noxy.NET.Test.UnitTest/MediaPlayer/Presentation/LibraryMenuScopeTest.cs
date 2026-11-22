using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class LibraryMenuScopeTest
{
    private static readonly Guid A = Guid.NewGuid();
    private static readonly Guid B = Guid.NewGuid();
    private static readonly Guid C = Guid.NewGuid();
    private static readonly Guid D = Guid.NewGuid();

    [Fact]
    public void Of_ForARowNotTicked_HasNoGroup()
    {
        LibraryMenuScope scope = LibraryMenuScope.Of(A, new HashSet<Guid>(), []);

        Assert.Equal(A, scope.ClickedID);
        Assert.False(scope.HasGroup);
        Assert.Empty(scope.GroupIDList);
    }

    [Fact]
    public void Of_ForTheOnlyTickedRow_HasNoGroup()
    {
        LibraryMenuScope scope = LibraryMenuScope.Of(A, new HashSet<Guid> { A }, [A]);

        Assert.False(scope.HasGroup);
        Assert.Empty(scope.GroupIDList);
    }

    [Fact]
    public void Of_ForARowInAGroupOfTwoOrMore_ReturnsTheGroupInPageOrder()
    {
        LibraryMenuScope scope = LibraryMenuScope.Of(B, new HashSet<Guid> { C, A, B }, [A, B, C]);

        Assert.True(scope.HasGroup);
        Assert.Equal([A, B, C], scope.GroupIDList);
    }

    [Fact]
    public void Of_ForANonTickedRow_WhileAGroupIsTicked_HasNoGroup()
    {
        LibraryMenuScope scope = LibraryMenuScope.Of(D, new HashSet<Guid> { A, B, C }, [A, B, C]);

        Assert.Equal(D, scope.ClickedID);
        Assert.False(scope.HasGroup);
        Assert.Empty(scope.GroupIDList);
    }
}
