using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Test_Case_Generator.Providers;

public static class GeminiClient {
    public static async Task<AiGenerationResult> GenerateAsync(HttpClient client, string apiKey, GenerateRequest request)
    {
        var model = string.IsNullOrWhiteSpace(request.Model) ? "gemini-3.6-flash" : request.Model;

        var body = new GeminiRequestBody(
            Contents: new List<GeminiContentPart> { new("user", new List<GeminiPart> { new(request.UserPrompt) }) },
            SystemInstruction: new GeminiSystemInstruction(new List<GeminiPart> { new(request.SystemPrompt) }),
            GenerationConfig: new GeminiGenerationConfig("application/json", 0.4)
        );

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync(url, body, AppJsonSerializerContext.Default.GeminiRequestBody);
        }
        catch (Exception ex)
        {
            return new AiGenerationResult(null, $"Failed to contact the Gemini API: {ex.Message}", 502);
        }

        var rawBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return new AiGenerationResult(null, $"Gemini API returned {(int)response.StatusCode}: {rawBody}", (int)response.StatusCode);
        }

        GeminiResponseBody? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize(rawBody, AppJsonSerializerContext.Default.GeminiResponseBody);
        }
        catch (Exception ex)
        {
            return new AiGenerationResult(null, $"Unexpected Gemini response format: {ex.Message}", 502);
        }

        var text = parsed?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        return string.IsNullOrWhiteSpace(text)
            ? new AiGenerationResult(null, "The Gemini API did not return any content.", 502)
            : new AiGenerationResult(text, null, 200);
    }
}

public record GeminiPart([property: JsonPropertyName("text")] string Text);
public record GeminiContentPart(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("parts")] List<GeminiPart> Parts
);
public record GeminiSystemInstruction([property: JsonPropertyName("parts")] List<GeminiPart> Parts);
public record GeminiGenerationConfig(
    [property: JsonPropertyName("responseMimeType")] string ResponseMimeType,
    [property: JsonPropertyName("temperature")] double Temperature
);
public record GeminiRequestBody(
    [property: JsonPropertyName("contents")] List<GeminiContentPart> Contents,
    [property: JsonPropertyName("systemInstruction")] GeminiSystemInstruction SystemInstruction,
    [property: JsonPropertyName("generationConfig")] GeminiGenerationConfig GenerationConfig
);
public record GeminiResponseContent([property: JsonPropertyName("parts")] List<GeminiPart>? Parts);
public record GeminiCandidate([property: JsonPropertyName("content")] GeminiResponseContent? Content);
public record GeminiResponseBody([property: JsonPropertyName("candidates")] List<GeminiCandidate>? Candidates);