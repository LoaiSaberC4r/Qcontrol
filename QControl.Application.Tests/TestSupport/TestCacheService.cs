using BuildingBlock.Application.Abstraction;

namespace QControl.Application.Tests.TestSupport;

internal sealed class TestCacheService : ICacheService
{
    private readonly Dictionary<string, object> _values = new();

    public int SetCallCount { get; private set; }
    public int InvalidateCallCount { get; private set; }

    public Task<(bool found, T? value)> TryGetAsync<T>(
        string key,
        CancellationToken ct = default)
    {
        var found = _values.TryGetValue(key, out var value) && value is T;
        return Task.FromResult((found, found ? (T)value! : default));
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl,
        IEnumerable<string> tags,
        CancellationToken ct = default)
    {
        SetCallCount++;
        _values[key] = value!;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _values.Remove(key);
        return Task.CompletedTask;
    }

    public Task InvalidateByTagsAsync(
        IEnumerable<string> tags,
        CancellationToken ct = default)
    {
        InvalidateCallCount++;
        _values.Clear();
        return Task.CompletedTask;
    }

    public Task ClearAllAsync(CancellationToken ct = default)
    {
        _values.Clear();
        return Task.CompletedTask;
    }
}
