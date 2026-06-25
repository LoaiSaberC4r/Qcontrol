using BuildingBlock.Domain.Primitive;
using QControl.Application.Abstraction.Presistence;

namespace QControl.Application.Tests.TestSupport;

internal sealed class InMemoryWriteRepository<TEntity>
    : IWriteRepository<TEntity>
    where TEntity : class
{
    private readonly List<TEntity> _items;

    public InMemoryWriteRepository(List<TEntity> items)
    {
        _items = items;
    }

    public int AddCallCount { get; private set; }

    public int UpdateCallCount { get; private set; }

    public int DeleteCallCount { get; private set; }

    public Task AddAsync(
        TEntity entity,
        CancellationToken ct = default)
    {
        AddCallCount++;
        _items.Add(entity);

        return Task.CompletedTask;
    }

    public Task AddRangeAsync(
        List<TEntity> entities,
        CancellationToken ct = default)
    {
        _items.AddRange(entities);

        return Task.CompletedTask;
    }

    public void Update(TEntity entity)
    {
        UpdateCallCount++;
    }

    public void UpdateRange(IEnumerable<TEntity> entities)
    {
        foreach (var _ in entities)
        {
            UpdateCallCount++;
        }
    }

    public void Delete(TEntity entity)
    {
        DeleteCallCount++;

        if (entity is ISoftDeleteEntity softDeleteEntity)
        {
            softDeleteEntity.IsDeleted = true;
            softDeleteEntity.DeletedOnUtc ??= DateTime.UtcNow;
            return;
        }

        _items.Remove(entity);
    }

    public void DeleteRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities.ToArray())
        {
            Delete(entity);
        }
    }
}
