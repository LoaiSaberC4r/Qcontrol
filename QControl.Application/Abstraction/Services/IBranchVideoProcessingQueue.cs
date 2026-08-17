namespace QControl.Application.Abstraction.Services;

public interface IBranchVideoProcessingQueue
{
    bool TryEnqueue(int branchVideoId);

    IAsyncEnumerable<int> ReadAllAsync(
        CancellationToken cancellationToken);
}
