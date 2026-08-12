using Microsoft.AspNetCore.Http;
using Backend.Models;
using Backend.Prompts;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// HttpClient theo từng provider — thêm Polly policy (retry/fallback) vào đây ở bước sau
builder.Services.AddHttpClient("groq");
builder.Services.AddHttpClient("gemini");

// 4 provider instance theo đúng thứ tự Fallback Chain (model lấy từ ModelConfig)
builder.Services.AddSingleton(sp => CreateGroq(sp, ModelConfig.Groq_Primary));
builder.Services.AddSingleton(sp => CreateGroq(sp, ModelConfig.Groq_Fallback));
builder.Services.AddSingleton(sp => CreateGemini(sp, ModelConfig.Gemini_Primary));
builder.Services.AddSingleton(sp => CreateGemini(sp, ModelConfig.Gemini_Fallback));

builder.Services.AddSingleton<FallbackChainService>();

var app = builder.Build();

// POST /api/chat — schema thành công theo docs/03-API-SPEC.md mục 1
app.MapPost("/api/chat", async (ChatRequest request, FallbackChainService fallback) =>
{
    var response = await fallback.GetChatCompletionAsync(request.Messages);
    return Results.Ok(response);
});

app.Run();

static GroqProviderService CreateGroq(IServiceProvider sp, string modelId) =>
    new(sp.GetRequiredService<IHttpClientFactory>().CreateClient("groq"),
        sp.GetRequiredService<IConfiguration>(), modelId);

static GeminiProviderService CreateGemini(IServiceProvider sp, string modelId) =>
    new(sp.GetRequiredService<IHttpClientFactory>().CreateClient("gemini"),
        sp.GetRequiredService<IConfiguration>(), modelId);