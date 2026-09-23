using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskForge.Api.Infrastructure.OpenAi;

public class ResponsesClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly int _timeoutSeconds;
    private readonly JsonSerializerOptions _jsonOptions;

    public ResponsesClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["OPENAI_API_KEY"] ?? throw new InvalidOperationException("OPENAI_API_KEY not configured");
        _model = config["OPENAI_MODEL"] ?? "gpt-4o-mini";
        _timeoutSeconds = int.Parse(config["OPENAI_TIMEOUT_SECONDS"] ?? "20");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    public async Task<OpenAiResponse> CallAsync(string systemPrompt, object input, object schema, CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

        var request = new OpenAiRequest(
            Model: _model,
            Input: new[]
            {
                new MessageInput("system", systemPrompt),
                new MessageInput("user", JsonSerializer.Serialize(input, _jsonOptions))
            },
            Text: new TextFormat(new JsonFormat(
                Type: "json_schema",
                Name: "schema_response",
                Strict: true,
                Schema: schema
            ))
        );

        var content = new StringContent(
            JsonSerializer.Serialize(request, _jsonOptions),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest, cts.Token);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cts.Token);
        return JsonSerializer.Deserialize<OpenAiResponse>(responseJson, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize OpenAI response");
    }

    public string ExtractTextFromResponse(OpenAiResponse response)
    {
        return response.Output?.FirstOrDefault()?.Content?.FirstOrDefault()?.Text
            ?? throw new InvalidOperationException("No text content in OpenAI response");
    }
}
