using EZ.Job.Core;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class EZJobServiceCollectionExtensions
{
    public static EZJobBuilder AddEZJob(this IServiceCollection services, Action<EZJobOptions>? configure = null)
    {
        var options = new EZJobOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<FireForgetChannel>();
        services.AddSingleton<RecoveryChannel>();
        services.AddSingleton<IJobStore, InMemoryJobStore>();
        services.AddSingleton<IRecurringStore, InMemoryRecurringStore>();
        services.AddSingleton<IJobDispatcher, JobDispatcher>();

        for (var i = 0; i < options.WorkerCount; i++)
        {
            services.AddSingleton<IHostedService>(sp =>
            {
                var channel = sp.GetRequiredService<FireForgetChannel>().Instance;
                var store = sp.GetRequiredService<IJobStore>();
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                return new JobWorker(channel, store, scopeFactory);
            });
        }

        for (var i = 0; i < options.RecoveryWorkerCount; i++)
        {
            services.AddSingleton<IHostedService>(sp =>
            {
                var channel = sp.GetRequiredService<RecoveryChannel>().Instance;
                var store = sp.GetRequiredService<IJobStore>();
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                return new JobWorker(channel, store, scopeFactory);
            });
        }

        services.AddSingleton<JobRecoveryService>();
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<JobRecoveryService>());

        return new EZJobBuilder(services, options);
    }
}
