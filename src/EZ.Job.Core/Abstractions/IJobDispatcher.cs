using System.Linq.Expressions;

namespace EZ.Job.Core;

public interface IJobDispatcher
{
    ValueTask EnqueueAsync<T>(Expression<Action<T>> methodCall, CancellationToken cancellationToken = default);
    ValueTask EnqueueAsync<T>(Expression<Func<T, Task>> methodCall, CancellationToken cancellationToken = default);
}
