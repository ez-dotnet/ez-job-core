namespace EZ.Job.Core;

public record class Job(
    string Id,
    string TypeName,
    string MethodName,
    string[] ArgumentTypes,
    object?[] Arguments,
    JobStatus Status,
    DateTime CreatedAt,
    string? Error,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? RecurringJobId = null);
