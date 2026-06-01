using System.Threading.Channels;

namespace EZ.Job.Core;

public sealed class FireForgetChannel
{
    public Channel<Job> Instance { get; } = Channel.CreateUnbounded<Job>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = false
    });
}
