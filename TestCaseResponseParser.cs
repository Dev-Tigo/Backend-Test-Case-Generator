using System.Text.Json;

public static class TestCaseResponseParser {
    public static (GenerateResponse? Result, string? Error) Parse(string rawText)
    {
        var cleaned = rawText.Trim();
        if (cleaned.StartsWith("```"))
        {
            var firstLineBreak = cleaned.IndexOf('\n');
            cleaned = firstLineBreak >= 0 ? cleaned[(firstLineBreak + 1)..] : cleaned;
            cleaned = cleaned.Replace("```", "").Trim();
        }

        GenerateResponse? result;
        try
        {
            result = JsonSerializer.Deserialize(cleaned, AppJsonSerializerContext.Default.GenerateResponse);
        }
        catch (Exception ex)
        {
            return (null, $"Could not parse the generated test cases: {ex.Message}");
        }

        if (result is null || result.TestCases.Count == 0)
        {
            return (null, "The AI returned an empty result.");
        }

        return (result, null);
    }
}
