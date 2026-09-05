using Test_Case_Generator.Providers;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://test-case-generator-5ln2.onrender.com/")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var geminiApiKey = builder.Configuration["Gemini:ApiKey"]
    ?? throw new InvalidOperationException("Gemini API key not found in configuration");
var groqApiKey = builder.Configuration["Groq:ApiKey"]
    ?? throw new InvalidOperationException("Groq API key not found in configuration");

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
    var client = httpClientFactory.CreateClient();
    var provider = string.IsNullOrWhiteSpace(request.Provider) ? "gemini" : request.Provider.ToLowerInvariant();

    var aiResult = provider switch
    {
        "groq" => await GroqClient.GenerateAsync(client, groqApiKey, request),
        "gemini" => await GeminiClient.GenerateAsync(client, geminiApiKey, request),
        _ => new AiGenerationResult(null, $"Unknown provider: {provider}", 400)
    };

    if (aiResult.Error is not null)
    {
        return Results.Json(new ErrorResponse(aiResult.Error), statusCode: aiResult.StatusCode);
    }

    var (result, parseError) = TestCaseResponseParser.Parse(aiResult.Text!);
    if (parseError is not null)
    {
        return Results.Json(new ErrorResponse(parseError), statusCode: 502);
    }

    return Results.Ok(result);
})
.WithName("GenerateTestCases");

app.Run();
