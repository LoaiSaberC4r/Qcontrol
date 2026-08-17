namespace QControl.Application.Abstraction.Services;

public interface IBranchVideoProcessingCoordinator
{
    ValueTask<IAsyncDisposable> AcquireAsync(
        int branchVideoId,
        CancellationToken cancellationToken);
}
