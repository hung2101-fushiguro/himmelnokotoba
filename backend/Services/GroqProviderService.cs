using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Backend.Models;

namespace Backend.Services;

public class GroqProviderService : IAiProviderService
{
    private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(8);

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelId;

    public GroqProviderService(HttpClient httpClient, IConfiguration configuration, string modelId)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = Timeout;
        _apiKey = configuration["GROQ_API_KEY"]
                  ?? throw new InvalidOperationException("Thiếu biến môi trường GROQ_API_KEY");
        _modelId = modelId;
    }

    public async Task<string> GetCompletionAsync(List<MessageDto> messages, string systemPrompt)
    {
        var request = new GroqChatRequest
        {
            Model = _modelId,
            Messages = messages
                .Prepend(new MessageDto { Role = "system", Content = systemPrompt })
                .Select(m => new GroqMessage { Role = m.Role, Content = m.Content })
                .ToList()
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        httpRequest.Content = JsonContent.Create(request);

        using var response = await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Groq API trả về {(int)response.StatusCode} {response.ReasonPhrase}: {errorBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<GroqChatResponse>()
                     ?? throw new HttpRequestException("Groq API trả về response rỗng");

        return result.Choices?.FirstOrDefault()?.Message?.Content
               ?? throw new HttpRequestException("Groq API trả về nhưng không có nội dung trả lời");
    }

    private sealed class GroqChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<GroqMessage> Messages { get; set; } = new();
    }

    private sealed class GroqMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private sealed class GroqChatResponse
    {
        [JsonPropertyName("choices")]
        public List<GroqChoice>? Choices { get; set; }
    }

    private sealed class GroqChoice
    {
        [JsonPropertyName("message")]
        public GroqMessage? Message { get; set; }
    }
}