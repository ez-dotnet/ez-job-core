using Microsoft.Extensions.Hosting;

namespace EZ.Job.Core;

internal sealed class JobRecoveryService : BackgroundService
{
    private readonly IJobStore _store;
    private readonly RecoveryChannel _channel;

    public JobRecoveryService(IJobStore store, RecoveryChannel channel)
    {
        _store = store;
        _channel = channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pending = await _store.GetPendingAsync(stoppingToken).ConfigureAwait(false);

        foreach (var job in pending)
        {
            if (job.RecurringJobId is not null) continue;
            _channel.Instance.Writer.TryWrite(job);
        }
    }
}
