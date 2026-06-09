using System.Collections.Concurrent;

namespace EZ.Job.Core;

public sealed class InMemoryJobStore : IJobStore
{
    private readonly ConcurrentDictionary<string, Job> _jobs = new(StringComparer.Ordinal);

    public ValueTask AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        _jobs[job.Id] = job;
        return default;
    }

    public ValueTask<Job?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = _jobs.TryGetValue(id, out var job) ? job : null;
        return new ValueTask<Job?>(result);
    }

    public ValueTask<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<IEnumerable<Job>>(_jobs.Values);
    }

    public ValueTask UpdateStatusAsync(string id, JobStatus status, string? error = null, CancellationToken cancellationToken = default)
    {
        if (_jobs.TryGetValue(id, out var job))
        {
            var now = DateTime.UtcNow;
            _jobs[id] = job with
            {
                Status = status,
                Error = error,
                StartedAt = status == JobStatus.Processing ? (job.StartedAt ?? now) : job.StartedAt,
                CompletedAt = status is JobStatus.Succeeded or JobStatus.Failed ? now : null
            };
        }
        return default;
    }

    public ValueTask<IEnumerable<Job>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<IEnumerable<Job>>([]);
    }
}
