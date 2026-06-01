namespace EZ.Job.Core;

public class EZJobOptions
{
    public int WorkerCount { get; set; } = 4;
    public int RecurringWorkerCount { get; set; } = 1;
    public int RecurringPollIntervalSeconds { get; set; } = 30;
    public int RecoveryWorkerCount { get; set; } = 1;
}
