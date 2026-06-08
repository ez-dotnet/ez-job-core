namespace EZ.Job.Core;

public interface IRecurringStore
{
    ValueTask AddOrUpdateAsync(RecurringDefinition definition, CancellationToken cancellationToken = default);
    ValueTask RemoveAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<RecurringDefinition?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<RecurringDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
}
