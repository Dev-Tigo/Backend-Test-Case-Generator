using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var geminiApiKey = builder.Configuration["Gemini:ApiKey"]
    ?? throw new InvalidOperationException("API key not found in configuration");

builder.Services.AddHttpClient();

var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var apiGroup = app.MapGroup("/api");

apiGroup.MapPost("/generate-test-cases", async (GenerateRequest request, IHttpClientFactory httpClientFactory) =>
{
    var model = string.IsNullOrWhiteSpace(request.Model) ? "gemini-3.6-flash" : request.Model;

    var geminiRequestBody = new GeminiRequestBody(
        Contents: new List<GeminiContentPart>
        {
            new("user", new List<GeminiPart> { new(request.UserPrompt) })
        },
        SystemInstruction: new GeminiSystemInstruction(new List<GeminiPart> { new(request.SystemPrompt) }),
        GenerationConfig: new GeminiGenerationConfig("application/json", 0.4)
    );

    var client = httpClientFactory.CreateClient();
    var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={geminiApiKey}";

    HttpResponseMessage httpResponse;
    try
    {
        httpResponse = await client.PostAsJsonAsync(url, geminiRequestBody, AppJsonSerializerContext.Default.GeminiRequestBody);
    }
    catch (Exception ex)
    {
        return Results.Json(new ErrorResponse($"Failed to contact the Gemini API: {ex.Message}"), statusCode: 502);
    }

    var rawBody = await httpResponse.Content.ReadAsStringAsync();

    if (!httpResponse.IsSuccessStatusCode)
    {
        return Results.Json(new ErrorResponse($"Gemini API returned {(int)httpResponse.StatusCode}: {rawBody}"), statusCode: (int)httpResponse.StatusCode);
    }

    GeminiResponseBody? geminiResponse;
    try
    {
        geminiResponse = JsonSerializer.Deserialize(rawBody, AppJsonSerializerContext.Default.GeminiResponseBody);
    }
    catch (Exception ex)
    {
        return Results.Json(new ErrorResponse($"Unexpected Gemini response format: {ex.Message}"), statusCode: 502);
    }

    var generatedText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

    if (string.IsNullOrWhiteSpace(generatedText))
    {
        return Results.Json(new ErrorResponse("The AI did not return any content."), statusCode: 502);
    }

    var cleanedText = generatedText.Trim();
    if (cleanedText.StartsWith("```"))
    {
        var firstLineBreak = cleanedText.IndexOf('\n');
        cleanedText = firstLineBreak >= 0 ? cleanedText[(firstLineBreak + 1)..] : cleanedText;
        cleanedText = cleanedText.Replace("```", "").Trim();
    }

    GenerateResponse? result;
    try
    {
        result = JsonSerializer.Deserialize(cleanedText, AppJsonSerializerContext.Default.GenerateResponse);
    }
    catch (Exception ex)
    {
        return Results.Json(new ErrorResponse($"Could not parse the generated test cases: {ex.Message}"), statusCode: 502);
    }

    if (result is null || result.TestCases.Count == 0)
    {
        return Results.Json(new ErrorResponse("The AI returned an empty result."), statusCode: 502);
    }

    return Results.Ok(result);
})
.WithName("GenerateTestCases");

app.Run();

public record GenerateRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("systemPrompt")] string SystemPrompt,
    [property: JsonPropertyName("userPrompt")] string UserPrompt
);

public record TestCase(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("titulo")] string Title,
    [property: JsonPropertyName("pre_condicoes")] string Preconditions,
    [property: JsonPropertyName("modulo")] string? Module,
    [property: JsonPropertyName("tipo")] string? Type,
    [property: JsonPropertyName("prioridade")] string? Priority,
    [property: JsonPropertyName("passos")] string[] Steps,
    [property: JsonPropertyName("dados_teste")] string TestData,
    [property: JsonPropertyName("resultado_esperado")] string ExpectedResult
);

public record GenerateResponse(
    [property: JsonPropertyName("casos_de_teste")] List<TestCase> TestCases
);

public record ErrorResponse([property: JsonPropertyName("error")] string Error);

// DTOs for talking to the Gemini API
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

[JsonSerializable(typeof(GenerateRequest))]
[JsonSerializable(typeof(TestCase))]
[JsonSerializable(typeof(List<TestCase>))]
[JsonSerializable(typeof(GenerateResponse))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(GeminiRequestBody))]
[JsonSerializable(typeof(GeminiResponseBody))]
internal partial class AppJsonSerializerContext : JsonSerializerContext {
}