using System.Collections.Concurrent;

namespace EZ.Job.Core;

public sealed class InMemoryRecurringStore : IRecurringStore
{
    private readonly ConcurrentDictionary<Guid, RecurringDefinition> _definitions = new();

    public ValueTask AddOrUpdateAsync(RecurringDefinition definition, CancellationToken cancellationToken = default)
    {
        _definitions[definition.Id] = definition;
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _definitions.TryRemove(id, out _);
        return ValueTask.CompletedTask;
    }

    public ValueTask<RecurringDefinition?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _definitions.TryGetValue(id, out var def);
        return ValueTask.FromResult(def);
    }

    public ValueTask<IEnumerable<RecurringDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult((IEnumerable<RecurringDefinition>)_definitions.Values);
    }

    public ValueTask SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        if (_definitions.TryGetValue(id, out var existing))
        {
            _definitions[id] = existing with { IsActive = isActive };
        }
        return ValueTask.CompletedTask;
    }
}
