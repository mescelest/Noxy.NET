using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class UnitOfWorkFactoryExtensionTest
{
    private readonly FakeStore _store = new();

    [Fact]
    public async Task RunWithKeyConflictRetry_ReturnsTheResultOfTheFirstSuccessfulRun()
    {
        int runs = 0;

        int result = await new FakeUnitOfWorkFactory(_store).RunWithKeyConflictRetry(_ =>
        {
            runs++;
            return Task.FromResult(42);
        }, TestContext.Current.CancellationToken);

        Assert.Equal(42, result);
        Assert.Equal(1, runs);
    }

    [Fact]
    public async Task RunWithKeyConflictRetry_RunsAgainInAFreshUnitOfWorkAfterAConflict()
    {
        int runs = 0;
        List<object> uowList = [];

        int result = await new FakeUnitOfWorkFactory(_store).RunWithKeyConflictRetry(uow =>
        {
            uowList.Add(uow);
            if (++runs == 1) throw new MetadataKeyConflictException();

            return Task.FromResult(runs);
        }, TestContext.Current.CancellationToken);

        Assert.Equal(2, result);
        Assert.Equal(2, uowList.Distinct().Count());
    }

    [Fact]
    public async Task RunWithKeyConflictRetry_GivesUpAfterTheMaximumAttempts_ThrowingTheConflict()
    {
        int runs = 0;

        await Assert.ThrowsAsync<MetadataKeyConflictException>(() => new FakeUnitOfWorkFactory(_store).RunWithKeyConflictRetry<int>(_ =>
        {
            runs++;
            throw new MetadataKeyConflictException();
        }, TestContext.Current.CancellationToken));

        Assert.Equal(UnitOfWorkFactoryExtension.MaxKeyConflictAttempts, runs);
    }

    [Fact]
    public async Task RunWithKeyConflictRetry_AnotherFailure_IsNotRetried()
    {
        int runs = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() => new FakeUnitOfWorkFactory(_store).RunWithKeyConflictRetry<int>(_ =>
        {
            runs++;
            throw new InvalidOperationException();
        }, TestContext.Current.CancellationToken));

        Assert.Equal(1, runs);
    }

    [Fact]
    public async Task RunWithKeyConflictRetry_ACancellation_StopsTheRetries()
    {
        using CancellationTokenSource source = new();
        int runs = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new FakeUnitOfWorkFactory(_store).RunWithKeyConflictRetry<int>(_ =>
        {
            runs++;
            source.Cancel();
            throw new MetadataKeyConflictException();
        }, source.Token));

        Assert.Equal(1, runs);
    }
}
