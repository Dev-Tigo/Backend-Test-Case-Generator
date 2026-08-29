using System.Text.Json.Serialization;

public record GenerateRequest(
    [property: JsonPropertyName("provider")] string? Provider,
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
