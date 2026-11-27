using Noxy.NET.MediaPlayer.Presentation.Services;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FileUseServiceTest
{
    private readonly FileUseService _service = new();
    private int _changedCount;

    public FileUseServiceTest()
    {
        _service.Changed += () => _changedCount++;
    }

    [Fact]
    public void TryBeginWrite_WhenNothingIsInUse_Allows()
    {
        Assert.True(_service.TryBeginWrite(Guid.NewGuid()));
    }

    [Fact]
    public async Task TryBeginWrite_ForTheFileInUse_Refuses_ButAnotherFileIsAllowed()
    {
        Guid used = Guid.NewGuid();
        await _service.Use(used);

        Assert.False(_service.TryBeginWrite(used));
        Assert.True(_service.TryBeginWrite(Guid.NewGuid()));
    }

    [Fact]
    public async Task TryBeginWrite_AfterTheFileIsNoLongerInUse_Allows()
    {
        Guid used = Guid.NewGuid();
        await _service.Use(used);

        await _service.Use(null);

        Assert.True(_service.TryBeginWrite(used));
    }

    [Fact]
    public async Task Use_WhileAWriteReplacesThatFile_WaitsForTheWriteToEnd()
    {
        Guid id = Guid.NewGuid();
        Assert.True(_service.TryBeginWrite(id));

        Task use = _service.Use(id);
        await Task.Delay(100, TestContext.Current.CancellationToken);

        Assert.False(use.IsCompleted);

        _service.EndWrite(id);
        await use;

        Assert.False(_service.TryBeginWrite(id));
    }

    [Fact]
    public async Task Use_WhileAWriteReplacesAnotherFile_DoesNotWait()
    {
        Guid writing = Guid.NewGuid();
        Assert.True(_service.TryBeginWrite(writing));

        await _service.Use(Guid.NewGuid()).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        _service.EndWrite(writing);
    }

    [Fact]
    public async Task Use_RaisesChanged_WhenAFileStopsBeingUsed_AndNotWhenNothingChanged()
    {
        Guid first = Guid.NewGuid();

        await _service.Use(first);
        Assert.Equal(0, _changedCount);

        await _service.Use(first);
        Assert.Equal(0, _changedCount);

        await _service.Use(Guid.NewGuid());
        Assert.Equal(1, _changedCount);

        await _service.Use(null);
        Assert.Equal(2, _changedCount);

        await _service.Use(null);
        Assert.Equal(2, _changedCount);
    }
}
