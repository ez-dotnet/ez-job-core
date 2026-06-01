namespace Microsoft.Extensions.DependencyInjection;

public class EZJobBuilder
{
    public IServiceCollection Services { get; }
    public EZ.Job.Core.EZJobOptions Options { get; }

    public EZJobBuilder(IServiceCollection services, EZ.Job.Core.EZJobOptions options)
    {
        Services = services;
        Options = options;
    }
}
