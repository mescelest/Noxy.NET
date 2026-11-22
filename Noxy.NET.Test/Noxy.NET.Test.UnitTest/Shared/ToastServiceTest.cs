using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.UI.Enums;
using Noxy.NET.UI.Models;
using Noxy.NET.UI.Services;

namespace Noxy.NET.Test.Shared;

public class ToastServiceTest
{
    private readonly FakeTimeProvider _time = new();
    private readonly ToastService _service;

    public ToastServiceTest()
    {
        _service = new(_time);
    }

    private static string Messages(List<Toast> list) => string.Join(",", list.Select(x => x.Message));

    #region -- Showing --

    [Fact]
    public void Show_PutsTheToastInTheVisibleList()
    {
        Toast toast = _service.Show("Saved", StatusEnum.Success, false);

        Assert.Equal([toast], _service.GetVisibleList());
        Assert.Equal("Saved", toast.Message);
        Assert.Equal(StatusEnum.Success, toast.Status);
    }

    [Fact]
    public void Show_RaisesChanged()
    {
        int count = 0;
        _service.Changed += () => count++;

        _service.Show("Saved", StatusEnum.Success, false);

        Assert.Equal(1, count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Show_RefusesAnEmptyMessage(string message)
    {
        Assert.Throws<ArgumentException>(() => _service.Show(message, StatusEnum.Info, false));
    }

    [Fact]
    public void Show_RefusesADurationOfZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Show("Saved", StatusEnum.Info, false, TimeSpan.Zero));
    }

    [Fact]
    public void Show_TheSameMessageAndStatusWhileItIsShown_ReturnsTheToastThatIsThere()
    {
        Toast first = _service.ShowError("Offline");

        Toast second = _service.ShowError("Offline");

        Assert.Same(first, second);
        Assert.Single(_service.GetVisibleList());
    }

    [Fact]
    public void Show_TheSameMessageWithAnotherStatus_IsAnotherToast()
    {
        _service.ShowError("Offline");
        _service.ShowWarning("Offline");

        Assert.Equal(2, _service.GetVisibleList().Count);
    }

    #endregion

    #region -- Permanence --

    [Fact]
    public void ShowError_IsPermanentByDefault()
    {
        Assert.True(_service.ShowError("Failed").IsPermanent);
    }

    [Fact]
    public void ShowWarningInfoAndSuccess_AreTemporaryByDefault()
    {
        Assert.False(_service.ShowWarning("a").IsPermanent);
        Assert.False(_service.ShowInfo("b").IsPermanent);
        Assert.False(_service.ShowSuccess("c").IsPermanent);
    }

    [Fact]
    public void ShowError_CanBeMadeTemporary()
    {
        Assert.False(_service.ShowError("Failed", false).IsPermanent);
    }

    [Fact]
    public void APermanentToast_StaysAfterAnyTime()
    {
        _service.ShowError("Failed");

        _time.Advance(TimeSpan.FromHours(1));

        Assert.Single(_service.GetVisibleList());
    }

    #endregion

    #region -- Expiry --

    [Fact]
    public void ATemporaryToast_ExpiresAfterTheDefaultDuration()
    {
        _service.DefaultDuration = TimeSpan.FromSeconds(5);
        _service.ShowInfo("Hello");

        _time.Advance(TimeSpan.FromSeconds(4));
        Assert.Single(_service.GetVisibleList());

        _time.Advance(TimeSpan.FromSeconds(1));
        Assert.Empty(_service.GetVisibleList());
    }

    [Fact]
    public void ATemporaryToast_ExpiresAfterItsOwnDuration()
    {
        _service.DefaultDuration = TimeSpan.FromSeconds(5);
        _service.ShowInfo("Hello", duration: TimeSpan.FromSeconds(1));

        _time.Advance(TimeSpan.FromSeconds(1));

        Assert.Empty(_service.GetVisibleList());
    }

    [Fact]
    public void Expiry_RaisesChanged()
    {
        _service.ShowInfo("Hello");
        int count = 0;
        _service.Changed += () => count++;

        _time.Advance(TimeSpan.FromSeconds(5));

        Assert.Equal(1, count);
    }

    #endregion

    #region -- Dismissing --

    [Fact]
    public void Dismiss_RemovesAShownToastAndItsTimer()
    {
        Toast toast = _service.ShowInfo("Hello");

        _service.Dismiss(toast.ID);

        Assert.Empty(_service.GetVisibleList());
        Assert.Equal(0, _time.ActiveTimerCount);
    }

    [Fact]
    public void Dismiss_AnUnknownId_ChangesNothing()
    {
        _service.ShowError("Failed");
        int count = 0;
        _service.Changed += () => count++;

        _service.Dismiss(Guid.NewGuid());

        Assert.Single(_service.GetVisibleList());
        Assert.Equal(0, count);
    }

    [Fact]
    public void Clear_RemovesShownAndWaitingToastsAndTheirTimers()
    {
        _service.MaxVisible = 1;
        _service.ShowInfo("a");
        _service.ShowInfo("b");

        _service.Clear();

        Assert.Empty(_service.GetVisibleList());
        Assert.Empty(_service.GetWaitingList());
        Assert.Equal(0, _time.ActiveTimerCount);
    }

    #endregion

    #region -- Queueing --

    [Fact]
    public void Show_MoreThanMaxVisible_MakesTheRestWait()
    {
        _service.MaxVisible = 2;

        _service.ShowError("a");
        _service.ShowError("b");
        _service.ShowError("c");

        Assert.Equal("a,b", Messages(_service.GetVisibleList()));
        Assert.Equal("c", Messages(_service.GetWaitingList()));
    }

    [Fact]
    public void Dismiss_LetsTheOldestWaitingToastIn()
    {
        _service.MaxVisible = 1;
        Toast first = _service.ShowError("a");
        _service.ShowError("b");
        _service.ShowError("c");

        _service.Dismiss(first.ID);

        Assert.Equal("b", Messages(_service.GetVisibleList()));
        Assert.Equal("c", Messages(_service.GetWaitingList()));
    }

    [Fact]
    public void Dismiss_AWaitingToast_RemovesItWithoutShowingIt()
    {
        _service.MaxVisible = 1;
        _service.ShowError("a");
        Toast waiting = _service.ShowError("b");

        _service.Dismiss(waiting.ID);

        Assert.Empty(_service.GetWaitingList());
        Assert.Equal("a", Messages(_service.GetVisibleList()));
    }

    [Fact]
    public void ATemporaryToast_OnlyStartsItsTimeOnceItIsShown()
    {
        _service.MaxVisible = 1;
        _service.DefaultDuration = TimeSpan.FromSeconds(5);
        _service.ShowInfo("a");
        _service.ShowInfo("b");

        _time.Advance(TimeSpan.FromSeconds(5));
        Assert.Equal("b", Messages(_service.GetVisibleList()));

        _time.Advance(TimeSpan.FromSeconds(4));
        Assert.Equal("b", Messages(_service.GetVisibleList()));

        _time.Advance(TimeSpan.FromSeconds(1));
        Assert.Empty(_service.GetVisibleList());
    }

    [Fact]
    public void RaisingMaxVisible_ShowsTheWaitingToasts()
    {
        _service.MaxVisible = 1;
        _service.ShowError("a");
        _service.ShowError("b");

        _service.MaxVisible = 2;

        Assert.Equal("a,b", Messages(_service.GetVisibleList()));
        Assert.Empty(_service.GetWaitingList());
    }

    [Fact]
    public void MaxVisible_RefusesLessThanOne()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.MaxVisible = 0);
    }

    #endregion
}
