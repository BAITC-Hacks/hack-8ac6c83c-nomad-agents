namespace TaskForge.Api.Domain;

public record AiLog(
    string Id,
    string Kind, // "analyze" | "score"
    string TaskId,
    string Model,
    string SystemPrompt,
    string InputJson,
    string RawOutput,
    string Validation, // "ok" | "retry-ok" | "fallback-stub"
    string[] Errors,
    int LatencyMs,
    DateTime CreatedAt
);
