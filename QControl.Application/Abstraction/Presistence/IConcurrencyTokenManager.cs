namespace QControl.Application.Abstraction.Presistence;

public interface IConcurrencyTokenManager
{
    void SetOriginalRowVersion<TEntity>(
        TEntity entity,
        byte[] rowVersion)
        where TEntity : class;
}
