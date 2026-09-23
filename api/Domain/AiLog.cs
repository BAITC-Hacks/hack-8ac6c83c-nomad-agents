namespace TaskForge.Api.Domain;

public static class AiLogValidationValues
{
    public const string Ok = "ok";
    public const string RetryOk = "retry-ok";
    public const string FallbackStub = "fallback-stub";
}

public sealed record AiLog(
    string Id,
    string Kind,
    string TaskId,
    string Model,
    string SystemPrompt,
    string Input,
    string RawOutput,
    string Validation,
    IReadOnlyList<string> Errors,
    long LatencyMs,
    DateTimeOffset CreatedAt);
