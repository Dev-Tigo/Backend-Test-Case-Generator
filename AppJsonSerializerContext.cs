using System.Text.Json.Serialization;

[JsonSerializable(typeof(GenerateRequest))]
[JsonSerializable(typeof(TestCase))]
[JsonSerializable(typeof(List<TestCase>))]
[JsonSerializable(typeof(GenerateResponse))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(GeminiRequestBody))]
[JsonSerializable(typeof(GeminiResponseBody))]
[JsonSerializable(typeof(GroqRequestBody))]
[JsonSerializable(typeof(GroqResponseBody))]
internal partial class AppJsonSerializerContext : JsonSerializerContext {
}