using System.Collections.Concurrent;
using QControl.Application.Abstraction.Services;

namespace QControl.infrastructure.Services;

internal sealed class BranchVideoProcessingCoordinator
    : IBranchVideoProcessingCoordinator
{
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();

    public async ValueTask<IAsyncDisposable> AcquireAsync(
        int branchVideoId,
        CancellationToken cancellationToken)
    {
        var semaphore = _locks.GetOrAdd(
            branchVideoId,
            static _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        return new Lease(semaphore);
    }

    private sealed class Lease : IAsyncDisposable
    {
        private SemaphoreSlim? _semaphore;

        public Lease(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public ValueTask DisposeAsync()
        {
            Interlocked.Exchange(ref _semaphore, null)?.Release();
            return ValueTask.CompletedTask;
        }
    }
}
