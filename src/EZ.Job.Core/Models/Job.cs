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
    string? RecurringJobId = null);
