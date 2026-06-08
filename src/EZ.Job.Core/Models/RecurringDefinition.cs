namespace EZ.Job.Core;

public record RecurringDefinition(
    Guid Id,
    string TypeName,
    string MethodName,
    string[] ArgumentTypes,
    object?[] Arguments,
    string CronExpression,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? LastExecutionUtc);
