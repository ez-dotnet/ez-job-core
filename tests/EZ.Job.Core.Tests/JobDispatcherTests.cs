using EZ.Job.Core;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EZ.Job.Core.Tests;

public sealed class JobDispatcherTests
{
    [Fact]
    public async Task EnqueueAsync_should_add_job_to_store()
    {
        var services = new ServiceCollection();
        services.AddEZJob();
        services.AddTransient<MyService>();

        var sp = services.BuildServiceProvider();
        var dispatcher = sp.GetRequiredService<IJobDispatcher>();
        var store = sp.GetRequiredService<IJobStore>();

        await dispatcher.EnqueueAsync<MyService>(s => s.DoSomething("test"));

        var jobs = await store.GetAllAsync();
        Assert.Single(jobs);
    }

    [Fact]
    public async Task EnqueueAsync_async_should_add_job_to_store()
    {
        var services = new ServiceCollection();
        services.AddEZJob();
        services.AddTransient<MyService>();

        var sp = services.BuildServiceProvider();
        var dispatcher = sp.GetRequiredService<IJobDispatcher>();
        var store = sp.GetRequiredService<IJobStore>();

        await dispatcher.EnqueueAsync<MyService>(s => s.DoSomethingAsync("test"));

        var jobs = await store.GetAllAsync();
        Assert.Single(jobs);
    }
}

public class MyService
{
    public void DoSomething(string msg) { }
    public Task DoSomethingAsync(string msg) => Task.CompletedTask;
}
