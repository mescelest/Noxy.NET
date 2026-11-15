using Noxy.NET.MediaPlayer.Application.Models.Upload;

namespace Noxy.NET.Test.MediaPlayer.Upload;

public class UploadGateTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly UploadGate _gate = new();

    [Fact]
    public void TryEnter_FirstCallSucceeds()
    {
        Assert.NotNull(_gate.TryEnter(UserID));
    }

    [Fact]
    public void TryEnter_SecondCallForTheSameUserFails()
    {
        using IDisposable? first = _gate.TryEnter(UserID);

        Assert.Null(_gate.TryEnter(UserID));
    }

    [Fact]
    public void TryEnter_DifferentUsersDoNotBlockEachOther()
    {
        using IDisposable? first = _gate.TryEnter(UserID);

        Assert.NotNull(_gate.TryEnter(Guid.CreateVersion7()));
    }

    [Fact]
    public void TryEnter_AfterTheLeaseIsDisposed_SucceedsAgain()
    {
        IDisposable lease = _gate.TryEnter(UserID) ?? throw new InvalidOperationException("The gate was not free.");
        lease.Dispose();

        Assert.NotNull(_gate.TryEnter(UserID));
    }

    [Fact]
    public void Dispose_Twice_DoesNotFreeASlotThatSomeoneElseNowHolds()
    {
        IDisposable first = _gate.TryEnter(UserID) ?? throw new InvalidOperationException("The gate was not free.");
        first.Dispose();
        using IDisposable? second = _gate.TryEnter(UserID);

        first.Dispose();

        Assert.Null(_gate.TryEnter(UserID));
    }
}
