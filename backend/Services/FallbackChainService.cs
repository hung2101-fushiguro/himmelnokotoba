using Backend.Models;
using Backend.Prompts;
using Polly;

namespace Backend.Services;

public class FallbackChainService
{
    private static readonly int MaxRetries = 1;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(1);
    private static readonly HashSet<string> AllowedTypes = new()
    {
        "kanji", "vocabulary", "grammar", "translation", "error_check", "general"
    };

    private readonly GroqProviderService _groqPrimary;
    private readonly GroqProviderService _groqFallback;
    private readonly GeminiProviderService _geminiPrimary;
    private readonly GeminiProviderService _geminiFallback;
    private readonly ILogger<FallbackChainService> _logger;

    public FallbackChainService(
        GroqProviderService groqPrimary,
        GroqProviderService groqFallback,
        GeminiProviderService geminiPrimary,
        GeminiProviderService geminiFallback,
        ILogger<FallbackChainService> logger)
    {
        _groqPrimary = groqPrimary;
        _groqFallback = groqFallback;
        _geminiPrimary = geminiPrimary;
        _geminiFallback = geminiFallback;
        _logger = logger;
    }

    public async Task<ChatResponse> GetChatCompletionAsync(List<MessageDto> messages)
    {
        var steps = new (string Label, Func<CancellationToken, Task<string>> Call)[]
        {
            ($"groq/{ModelConfig.Groq_Primary}", ct => _groqPrimary.GetCompletionAsync(messages, SystemPrompts.JapaneseTutor)),
            ($"groq/{ModelConfig.Groq_Fallback}", ct => _groqFallback.GetCompletionAsync(messages, SystemPrompts.JapaneseTutor)),
            ($"gemini/{ModelConfig.Gemini_Primary}", ct => _geminiPrimary.GetCompletionAsync(messages, SystemPrompts.JapaneseTutor)),
            ($"gemini/{ModelConfig.Gemini_Fallback}", ct => _geminiFallback.GetCompletionAsync(messages, SystemPrompts.JapaneseTutor)),
        };

        var tracker = new ProviderResultTracker();

        string raw;
        try
        {
            raw = await ExecuteChainAsync(steps, 0, tracker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallback Chain: TẤT CẢ {ProviderCount} provider đều thất bại, trả lỗi 503 cho Client.", steps.Length);
            throw new AllProvidersFailedException("Tất cả AI Provider đều thất bại.", ex);
        }

        _logger.LogInformation("Fallback Chain: Request thành công, provider đã dùng: {ProviderUsed}.", tracker.ProviderUsed);

        var (type, content) = ParseResponse(raw);
        return new ChatResponse
        {
            Success = true,
            Type = type,
            Content = content,
            ProviderUsed = tracker.ProviderUsed
        };
    }

    private async Task<string> ExecuteChainAsync(
        (string Label, Func<CancellationToken, Task<string>> Call)[] steps,
        int index,
        ProviderResultTracker tracker)
    {
        var step = steps[index];
        var isLast = index == steps.Length - 1;

        var retry = Policy<string>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(MaxRetries, _ => RetryDelay);

        AsyncPolicy<string> policy;
        if (isLast)
        {
            policy = retry;
        }
        else
        {
            var nextIndex = index + 1;
            var fallback = Policy<string>
                .Handle<Exception>()
                .FallbackAsync(
                    fallbackAction: ct => ExecuteChainAsync(steps, nextIndex, tracker),
                    onFallbackAsync: outcome =>
                    {
                        _logger.LogWarning(outcome.Exception,
                            "Fallback Chain: Provider {FailedProvider} fail/timeout sau {Attempts} lần thử, rớt xuống provider {NextProvider}.",
                            step.Label, MaxRetries + 1, steps[nextIndex].Label);
                        return Task.CompletedTask;
                    });
            policy = fallback.WrapAsync(retry);
        }

        return await policy.ExecuteAsync(
            new Func<CancellationToken, Task<string>>(
                ct => { tracker.ProviderUsed = step.Label; return step.Call(ct); }),
            CancellationToken.None);
    }

    private static (string type, string content) ParseResponse(string raw)
    {
        var lines = raw.Replace("\r\n", "\n").Split('\n');
        var contentLines = new List<string>();
        var type = "general";

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (contentLines.Count == 0 && trimmed.StartsWith("TYPE:", StringComparison.OrdinalIgnoreCase))
            {
                var candidate = trimmed["TYPE:".Length..].Trim().ToLower();
                type = AllowedTypes.Contains(candidate) ? candidate : "general";
                continue;
            }
            contentLines.Add(line);
        }

        return (type, string.Join("\n", contentLines).Trim());
    }

    private sealed class ProviderResultTracker
    {
        public string ProviderUsed { get; set; } = string.Empty;
    }
}