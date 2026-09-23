using System.Net.Http.Headers;
using System.Text.Json;

namespace TaskForge.Api.Infrastructure.OpenAi;

public sealed record ResponseAttempt(string RawOutput, string? Text, string? Error);

public sealed class ResponsesClient(HttpClient http, IConfiguration configuration)
{
    public string Model => configuration["OPENAI_MODEL"] ?? "";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(Model) && !string.IsNullOrWhiteSpace(configuration["OPENAI_API_KEY"]);

    public string Redact(string value)
    {
        var key = configuration["OPENAI_API_KEY"];
        return string.IsNullOrEmpty(key) ? value : value.Replace(key, "[REDACTED]", StringComparison.Ordinal);
    }

    public async Task<ResponseAttempt> AnalyzeAsync(string prompt, string input, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", configuration["OPENAI_API_KEY"]);
        request.Content = JsonContent.Create(new
        {
            model = Model,
            input = new[] { new { role = "system", content = prompt }, new { role = "user", content = input } },
            text = new { format = new { type = "json_schema", name = "task_analysis", strict = true, schema = Schemas.Analysis } }
        });
        var raw = "";
        try
        {
            using var response = await http.SendAsync(request, cancellationToken);
            raw = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode) return new(raw, null, $"OpenAI HTTP {(int)response.StatusCode}.");
            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return new(raw, null, "Invalid response envelope.");
            if (root.TryGetProperty("status", out var status) && status.GetString() != "completed")
                return new(raw, null, "Response did not complete.");
            List<string> texts = [];
            if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
                foreach (var item in output.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("type", out var type) || type.GetString() != "message" ||
                        !item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array) continue;
                    foreach (var part in content.EnumerateArray())
                    {
                        if (part.ValueKind != JsonValueKind.Object || !part.TryGetProperty("type", out var kind)) continue;
                        if (kind.GetString() == "refusal") return new(raw, null, "Model refused analysis.");
                        if (kind.GetString() == "output_text" && part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                            texts.Add(text.GetString()!);
                    }
                }
            return texts.Count == 0 ? new(raw, null, "Response contains no output text.") : new(raw, string.Concat(texts), null);
        }
        catch (HttpRequestException) { return new(raw, null, "OpenAI network failure."); }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        { return new(raw, null, "OpenAI request timed out."); }
        catch (JsonException) { return new(raw, null, "Malformed response envelope."); }
        catch (InvalidOperationException) { return new(raw, null, "Invalid response envelope types."); }
    }
}
