using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Channels;

namespace EZ.Job.Core;

public sealed class JobDispatcher : IJobDispatcher
{
    private readonly Channel<Job> _channel;
    private readonly IJobStore _store;

    public JobDispatcher(FireForgetChannel channel, IJobStore store)
    {
        _channel = channel.Instance;
        _store = store;
    }

    public ValueTask EnqueueAsync<T>(Expression<Action<T>> methodCall, CancellationToken cancellationToken = default)
    {
        var (method, args) = ExpressionHelpers.Extract(methodCall);
        return EnqueueCoreAsync<T>(method, args, cancellationToken);
    }

    public ValueTask EnqueueAsync<T>(Expression<Func<T, Task>> methodCall, CancellationToken cancellationToken = default)
    {
        var (method, args) = ExpressionHelpers.Extract(methodCall);
        return EnqueueCoreAsync<T>(method, args, cancellationToken);
    }

    private async ValueTask EnqueueCoreAsync<T>(MethodInfo method, object?[] args, CancellationToken cancellationToken)
    {
        var parameterTypes = method.GetParameters()
            .Select(p => p.ParameterType.FullName!)
            .ToArray();

        var job = new Job(
            Guid.NewGuid().ToString("N"),
            typeof(T).FullName!,
            method.Name,
            parameterTypes,
            args,
            JobStatus.Enqueued,
            DateTime.UtcNow,
            Error: null,
            StartedAt: null,
            CompletedAt: null,
            RecurringJobId: null);

        await _store.AddAsync(job, cancellationToken).ConfigureAwait(false);
        _channel.Writer.TryWrite(job);
    }
}
