using QControl.Application.Abstraction.Presistence;

namespace QControl.Application.Tests.TestSupport;

internal sealed class TestConcurrencyTokenManager : IConcurrencyTokenManager
{
    public int SetOriginalRowVersionCallCount { get; private set; }

    public byte[]? LastRowVersion { get; private set; }

    public void SetOriginalRowVersion<TEntity>(
        TEntity entity,
        byte[] rowVersion)
        where TEntity : class
    {
        SetOriginalRowVersionCallCount++;
        LastRowVersion = rowVersion;
    }
}
