using System.Linq.Expressions;
using BuildingBlock.Domain.Primitive;
using BuildingBlock.Domain.Specification;
using Microsoft.EntityFrameworkCore.Query;
using QControl.Application.Abstraction.Presistence;

namespace QControl.Application.Tests.TestSupport;

internal sealed class InMemoryWriteReadRepository<TEntity>
    : IWriteReadRepository<TEntity>
    where TEntity : class
{
    private readonly List<TEntity> _items;

    public InMemoryWriteReadRepository(List<TEntity> items)
    {
        _items = items;
    }

    public int AnyCallCount { get; private set; }

    public int FirstOrDefaultSpecCallCount { get; private set; }

    public int FirstOrDefaultProjectionSpecCallCount { get; private set; }

    public int ListProjectionSpecCallCount { get; private set; }

    public int ListWithCountCallCount { get; private set; }

    public int QueryCallCount { get; private set; }

    public Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
        => Task.FromResult(FindById(id));

    public Task<TEntity?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
        => Task.FromResult(FindById(id));

    public Task<TEntity?> GetByIdAsync(
        long id,
        CancellationToken ct = default)
        => Task.FromResult(FindById(id));

    public Task<TEntity?> GetByIdAsync(
        string id,
        CancellationToken ct = default)
        => Task.FromResult(FindById(id));

    public Task<TEntity?> GetByIdTrackedAsync(
        Guid id,
        CancellationToken ct = default)
        => Task.FromResult(FindById(id));

    public Task<TEntity?> GetByIdTrackedAsync(
        int id,
        CancellationToken ct = default)
        => Task.FromResult(FindById(id));

    public Task<TEntity?> GetByIdTrackedAsync(
        long id,
        CancellationToken ct = default)
        => Task.FromResult(FindById(id));

    public Task<TEntity?> GetByIdTrackedAsync(
        string id,
        CancellationToken ct = default)
        => Task.FromResult(FindById(id));

    public Task<TEntity?> GetByPropertyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
        => Task.FromResult(ApplyGlobalFilter(_items)
            .FirstOrDefault(predicate.Compile()));

    public Task<TEntity?> GetByPropertyTrackedAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
        => Task.FromResult(ApplyGlobalFilter(_items)
            .FirstOrDefault(predicate.Compile()));

    public Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
    {
        AnyCallCount++;

        return Task.FromResult(ApplyGlobalFilter(_items)
            .Any(predicate.Compile()));
    }

    public Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken ct = default)
    {
        var count = predicate is null
            ? ApplyGlobalFilter(_items).Count()
            : ApplyGlobalFilter(_items).Count(predicate.Compile());

        return Task.FromResult(count);
    }

    public Task<List<TEntity>> ListAsync(
        Specification<TEntity> spec,
        CancellationToken ct = default)
        => Task.FromResult(ApplyDataSpec(spec).ToList());

    public Task<TEntity?> FirstOrDefaultAsync(
        Specification<TEntity> spec,
        CancellationToken ct = default)
    {
        FirstOrDefaultSpecCallCount++;

        return Task.FromResult(ApplyDataSpec(spec).FirstOrDefault());
    }

    public Task<List<TOut>> ListAsync<TOut>(
        Specification<TEntity, TOut> spec,
        CancellationToken ct = default)
    {
        ListProjectionSpecCallCount++;

        return Task.FromResult(ApplyDataSpec(spec)
            .Select(spec.Selector.Compile())
            .ToList());
    }

    public Task<TOut?> FirstOrDefaultAsync<TOut>(
        Specification<TEntity, TOut> spec,
        CancellationToken ct = default)
    {
        FirstOrDefaultProjectionSpecCallCount++;

        var result = ApplyDataSpec(spec)
            .Select(spec.Selector.Compile())
            .FirstOrDefault();

        return Task.FromResult(result);
    }

    public IQueryable<TEntity> Query()
    {
        QueryCallCount++;
        return new TestAsyncEnumerable<TEntity>(_items);
    }

    public Task<(List<TEntity> Data, int Count)> ListWithCountAsync(
        Specification<TEntity> spec,
        CancellationToken ct = default)
    {
        ListWithCountCallCount++;

        var count = CountForSpec(spec);
        var data = ApplyDataSpec(spec).ToList();

        return Task.FromResult((data, count));
    }

    public Task<(List<TOut> Data, int Count)> ListWithCountAsync<TOut>(
        Specification<TEntity, TOut> spec,
        CancellationToken ct = default)
    {
        ListWithCountCallCount++;

        var count = CountForSpec(spec);
        var data = ApplyDataSpec(spec)
            .Select(spec.Selector.Compile())
            .ToList();

        return Task.FromResult((data, count));
    }

    private TEntity? FindById(object id)
    {
        var property = typeof(TEntity).GetProperty("Id");

        return ApplyGlobalFilter(_items).FirstOrDefault(item =>
            Equals(property!.GetValue(item), id));
    }

    private int CountForSpec(Specification<TEntity> spec)
    {
        var source = spec.IsGlobalFiltersIgnored
            ? _items
            : ApplyGlobalFilter(_items);

        return source.Count(spec.Criteria.Compile());
    }

    private IEnumerable<TEntity> ApplyDataSpec(Specification<TEntity> spec)
    {
        IEnumerable<TEntity> query = spec.IsGlobalFiltersIgnored
            ? _items
            : ApplyGlobalFilter(_items);

        query = query.Where(spec.Criteria.Compile());

        query = ApplyOrdering(query, spec);

        if (spec.IsPagingEnabled)
        {
            query = query
                .Skip(spec.Skip)
                .Take(spec.Take);
        }

        return query;
    }

    private static IEnumerable<TEntity> ApplyGlobalFilter(
        IEnumerable<TEntity> source)
    {
        if (!typeof(ISoftDeleteEntity).IsAssignableFrom(typeof(TEntity)))
        {
            return source;
        }

        return source.Where(item =>
            item is not ISoftDeleteEntity softDeleteEntity ||
            !softDeleteEntity.IsDeleted);
    }

    private static IEnumerable<TEntity> ApplyOrdering(
        IEnumerable<TEntity> source,
        Specification<TEntity> spec)
    {
        if (spec.OrderByDescendingExpressions.Count > 0)
        {
            IOrderedEnumerable<TEntity>? ordered = null;

            foreach (var expression in spec.OrderByDescendingExpressions)
            {
                var compiled = expression.Compile();
                ordered = ordered is null
                    ? source.OrderByDescending(compiled)
                    : ordered.ThenByDescending(compiled);
            }

            return ordered!;
        }

        if (spec.OrderByExpressions.Count > 0)
        {
            IOrderedEnumerable<TEntity>? ordered = null;

            foreach (var expression in spec.OrderByExpressions)
            {
                var compiled = expression.Compile();
                ordered = ordered is null
                    ? source.OrderBy(compiled)
                    : ordered.ThenBy(compiled);
            }

            return ordered!;
        }

        return source;
    }
}

internal sealed class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
        => new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(Expression expression)
        => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression)
        => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(
        Expression expression,
        CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethods()
            .Single(method =>
                method.Name == nameof(IQueryProvider.Execute) &&
                method.IsGenericMethod)
            .MakeGenericMethod(resultType)
            .Invoke(_inner, new object[] { expression });

        return (TResult)typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal sealed class TestAsyncEnumerable<T>
    : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
    }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(
        CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider =>
        new TestAsyncQueryProvider<T>(this);
}

internal sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
        => ValueTask.FromResult(_inner.MoveNext());

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}
