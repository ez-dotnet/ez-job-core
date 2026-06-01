namespace EZ.Job.Core;

public interface IJobStore
{
    ValueTask AddAsync(Job job, CancellationToken cancellationToken = default);
    ValueTask<Job?> GetAsync(string id, CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask UpdateStatusAsync(string id, JobStatus status, string? error = null, CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<Job>> GetPendingAsync(CancellationToken cancellationToken = default);
}
