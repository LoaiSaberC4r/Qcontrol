using System.Threading.Channels;
using QControl.Application.Abstraction.Services;

namespace QControl.infrastructure.Services;

internal sealed class BranchVideoProcessingQueue
    : IBranchVideoProcessingQueue
{
    private readonly Channel<int> _channel = Channel.CreateUnbounded<int>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

    public bool TryEnqueue(int branchVideoId) =>
        branchVideoId > 0 && _channel.Writer.TryWrite(branchVideoId);

    public IAsyncEnumerable<int> ReadAllAsync(
        CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
