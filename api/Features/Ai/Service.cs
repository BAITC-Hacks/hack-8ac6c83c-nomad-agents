using System.Diagnostics;
using System.Text.Json;
using TaskForge.Api.Domain;
using TaskForge.Api.Infrastructure.InMemory;
using TaskForge.Api.Infrastructure.OpenAi;

namespace TaskForge.Api.Features.Ai;

public sealed class AnalysisService(ResponsesClient client, IAiLogRepository logs, IConfiguration configuration)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public Task<AnalysisDto> AnalyzeAsync(TaskCard task, CancellationToken cancellationToken = default) =>
        AnalyzeAsync(task.Id, task.RawDraft, task.Industry, task.Fields, cancellationToken);

    public async Task<AnalysisDto> AnalyzeAsync(string taskId, string rawDraft, string industry,
        TaskFields fields, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var input = JsonSerializer.Serialize(new { draft = rawDraft, industry, currentFields = fields,
            fieldCatalog = FieldCatalog.All.Select(f => new { key = f.Key, meaning = f.Meaning }) }, Json);
        var prompt = AnalysisPrompt.System;
        var elapsed = Stopwatch.StartNew();
        IReadOnlyList<string> errors = [];
        var stubMode = string.Equals(configuration["AI_MODE"], "stub", StringComparison.OrdinalIgnoreCase);
        if (!stubMode && client.IsConfigured)
        {
            using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            budget.CancelAfter(TimeSpan.FromSeconds(20));
            for (var attempt = 0; attempt < 2 && !budget.IsCancellationRequested; attempt++)
            {
                var started = Stopwatch.StartNew();
                ResponseAttempt response;
                try { response = await client.AnalyzeAsync(prompt, input, budget.Token); }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                { response = new("", null, "Analysis exceeded the 20-second budget."); }
                var validation = response.Error is null
                    ? AnalyzeValidator.Validate(response.Text!, rawDraft)
                    : new AnalysisValidation(null, [response.Error]);
                errors = validation.Errors;
                WriteLog(prompt, response.RawOutput, validation.Analysis is null ? AiLogValidationValues.Invalid :
                    attempt == 0 ? AiLogValidationValues.Ok : AiLogValidationValues.RetryOk, errors, started.ElapsedMilliseconds, client.Model);
                if (validation.Analysis is not null) return validation.Analysis;
                prompt = AnalysisPrompt.System + "\nPrevious output invalid: " + string.Join(" ", errors);
            }
        }
        else errors = [stubMode ? "AI_MODE=stub; network skipped." : "OpenAI model/key not configured; network skipped."];
        cancellationToken.ThrowIfCancellationRequested();
        var fallback = AnalyzeStub.Create(fields);
        WriteLog(AnalysisPrompt.System, JsonSerializer.Serialize(fallback, Json), AiLogValidationValues.FallbackStub,
            errors, elapsed.ElapsedMilliseconds, "stub");
        return fallback;

        void WriteLog(string systemPrompt, string raw, string validation, IReadOnlyList<string> issues, long latency, string model) =>
            logs.Add(new(Guid.NewGuid().ToString("N"), "analyze", taskId, client.Redact(model), client.Redact(systemPrompt),
                client.Redact(input), client.Redact(raw), validation, issues.Select(client.Redact).ToArray(), latency, DateTimeOffset.UtcNow));
    }
}
