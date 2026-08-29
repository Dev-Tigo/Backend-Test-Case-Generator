using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Test_Case_Generator.Providers;

public static class GroqClient {
    private const string Url = "https://api.groq.com/openai/v1/chat/completions";
    private const int MaxAttempts = 2;

    public static async Task<AiGenerationResult> GenerateAsync(HttpClient client, string apiKey, GenerateRequest request)
    {
        var model = string.IsNullOrWhiteSpace(request.Model) ? "openai/gpt-oss-120b" : request.Model;
        var reasoningEffort = model.StartsWith("openai/gpt-oss") ? "low" : null;

        var body = new GroqRequestBody(
            Model: model,
            Messages: new List<GroqMessage>
            {
                new("system", request.SystemPrompt),
                new("user", request.UserPrompt)
            },
            ResponseFormat: new GroqResponseFormat("json_object"),
            Temperature: 0.4,
            MaxCompletionTokens: 4096,
            ReasoningEffort: reasoningEffort
        );

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = JsonContent.Create(body, AppJsonSerializerContext.Default.GroqRequestBody)
            };
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(httpRequest);
            }
            catch (Exception ex)
            {
                return new AiGenerationResult(null, $"Failed to contact the Groq API: {ex.Message}", 502);
            }

            var rawBody = await response.Content.ReadAsStringAsync();

            var isRetryableJsonFailure = response.StatusCode == System.Net.HttpStatusCode.BadRequest
                && rawBody.Contains("json_validate_failed");

            if (!response.IsSuccessStatusCode)
            {
                if (isRetryableJsonFailure && attempt < MaxAttempts)
                {
                    continue;
                }
                return new AiGenerationResult(null, $"Groq API returned {(int)response.StatusCode}: {rawBody}", (int)response.StatusCode);
            }

            GroqResponseBody? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize(rawBody, AppJsonSerializerContext.Default.GroqResponseBody);
            }
            catch (Exception ex)
            {
                return new AiGenerationResult(null, $"Unexpected Groq response format: {ex.Message}", 502);
            }

            var text = parsed?.Choices?.FirstOrDefault()?.Message?.Content;
            if (!string.IsNullOrWhiteSpace(text))
            {
                return new AiGenerationResult(text, null, 200);
            }

            if (attempt == MaxAttempts)
            {
                return new AiGenerationResult(null, "The Groq API did not return any content.", 502);
            }
        }

        return new AiGenerationResult(null, "The Groq API did not return any content after retries.", 502);
    }
}

public record GroqMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content
);
public record GroqResponseFormat([property: JsonPropertyName("type")] string Type);
public record GroqRequestBody(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] List<GroqMessage> Messages,
    [property: JsonPropertyName("response_format")] GroqResponseFormat ResponseFormat,
    [property: JsonPropertyName("temperature")] double Temperature,
    [property: JsonPropertyName("max_completion_tokens")] int MaxCompletionTokens,
    [property: JsonPropertyName("reasoning_effort"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ReasoningEffort
);
public record GroqResponseMessage([property: JsonPropertyName("content")] string? Content);
public record GroqChoice([property: JsonPropertyName("message")] GroqResponseMessage? Message);
public record GroqResponseBody([property: JsonPropertyName("choices")] List<GroqChoice>? Choices);