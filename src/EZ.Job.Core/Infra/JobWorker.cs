using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EZ.Job.Core;

public sealed class JobWorker : BackgroundService
{
    private readonly Channel<Job> _channel;
    private readonly IJobStore _store;
    private readonly IServiceScopeFactory _scopeFactory;

    public JobWorker(Channel<Job> channel, IJobStore store, IServiceScopeFactory scopeFactory)
    {
        _channel = channel;
        _store = store;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reader = _channel.Reader;

        while (await reader.WaitToReadAsync(stoppingToken).ConfigureAwait(false))
        {
            while (reader.TryRead(out var job))
            {
                await ProcessJobAsync(job, stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task ProcessJobAsync(Job job, CancellationToken cancellationToken)
    {
        await _store.UpdateStatusAsync(job.Id, JobStatus.Processing, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var type = ResolveType(job.TypeName);
            var instance = scope.ServiceProvider.GetRequiredService(type);

            var argTypes = job.ArgumentTypes.Select(ResolveType).ToArray();
            var method = type.GetMethod(job.MethodName, argTypes)!;

            var invoker = ExpressionHelpers.GetOrCreateInvoker(method);
            var result = invoker(instance, job.Arguments);

            if (result is Task task)
            {
                await task.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            await _store.UpdateStatusAsync(job.Id, JobStatus.Succeeded, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await _store.UpdateStatusAsync(job.Id, JobStatus.Failed, ex.ToString(), cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static Type ResolveType(string typeName)
    {
        return Type.GetType(typeName, throwOnError: false)
            ?? AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(typeName))
                .FirstOrDefault(t => t is not null)
            ?? throw new InvalidOperationException($"Type '{typeName}' could not be resolved.");
    }
}
