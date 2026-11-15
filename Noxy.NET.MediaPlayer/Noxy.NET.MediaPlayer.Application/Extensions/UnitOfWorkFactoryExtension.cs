using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.MediaPlayer.Application.Extensions;

public static class UnitOfWorkFactoryExtension
{
    public const int MaxKeyConflictAttempts = 3;

    extension(IUnitOfWorkFactory factoryUnitOfWork)
    {
        // Runs the whole use case in a fresh unit of work, and again in another one when its commit loses a race to create the same item (a unique-key conflict). The second run finds the winner's item instead of adding its own. The work commits itself.
        public async Task<T> RunWithKeyConflictRetry<T>(Func<IUnitOfWork, Task<T>> work, CancellationToken cancellationToken)
        {
            for (int attempt = 1;; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await using IUnitOfWork uow = factoryUnitOfWork.Create();
                try
                {
                    return await work(uow);
                }
                catch (MetadataKeyConflictException) when (attempt < MaxKeyConflictAttempts)
                {
                    // Another request created the same item first: the next run finds it.
                }
            }
        }
    }
}
