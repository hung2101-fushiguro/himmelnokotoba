using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Backend.Models;

namespace Backend.Services;

public class GeminiProviderService : IAiProviderService
{
    private const string EndpointTemplate =
        "https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(8);

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelId;

    public GeminiProviderService(HttpClient httpClient, IConfiguration configuration, string modelId)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = Timeout;
        _apiKey = configuration["GEMINI_API_KEY"]
                  ?? throw new InvalidOperationException("Thiếu biến môi trường GEMINI_API_KEY");
        _modelId = modelId;
    }

    public async Task<string> GetCompletionAsync(List<MessageDto> messages, string systemPrompt)
    {
        var request = new GeminiGenerateContentRequest
        {
            SystemInstruction = new GeminiContent
            {
                Parts = new List<GeminiPart> { new() { Text = systemPrompt } }
            },
            Contents = messages
                .Select(m => new GeminiContent
                {
                    Role = ConvertRole(m.Role),
                    Parts = new List<GeminiPart> { new() { Text = m.Content } }
                })
                .ToList()
        };

        var url = string.Format(EndpointTemplate, _modelId) + $"?key={_apiKey}";
        using var response = await _httpClient.PostAsJsonAsync(url, request);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Gemini API trả về {(int)response.StatusCode} {response.ReasonPhrase}: {errorBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<GeminiGenerateContentResponse>()
                     ?? throw new HttpRequestException("Gemini API trả về response rỗng");

        return result.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
               ?? throw new HttpRequestException("Gemini API trả về nhưng không có nội dung trả lời");
    }

    private static string ConvertRole(string role) => role switch
    {
        "assistant" => "model",
        _ => "user"
    };

    private sealed class GeminiGenerateContentRequest
    {
        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = new();

        [JsonPropertyName("systemInstruction")]
        public GeminiContent? SystemInstruction { get; set; }
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("parts")]
        public List<GeminiPart>? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private sealed class GeminiGenerateContentResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }
}