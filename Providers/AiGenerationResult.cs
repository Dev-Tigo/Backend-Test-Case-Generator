namespace Test_Case_Generator.Providers;

/// <summary>
/// Resultado unificado da geração de casos de teste via IA.
/// </summary>
public record AiGenerationResult(
    string? Text,
    string? Error,
    int StatusCode
);
