using Microsoft.AspNetCore.Http;
using Backend.Models;
using Backend.Prompts;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS: chỉ cho phép đúng domain Frontend (docs/03-API-SPEC.md mục 5)
var allowedOrigin = builder.Configuration["ALLOWED_ORIGIN"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        if (!string.IsNullOrWhiteSpace(allowedOrigin))
        {
            // Production: chỉ cho phép đúng domain từ biến môi trường ALLOWED_ORIGIN
            policy.WithOrigins(allowedOrigin)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // FALLBACK CHỈ DÀNH CHO DEV: ALLOWED_ORIGIN chưa được set (môi trường
            // local), tạm cho phép localhost với port bất kỳ để test Frontend chạy
            // trên máy. KHÔNG được dùng fallback này ở môi trường production.
            policy.SetIsOriginAllowed(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                (uri.Host is "localhost" or "127.0.0.1"));
        }
    });
});

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

app.UseCors("frontend");

// POST /api/chat — schema thành công theo docs/03-API-SPEC.md mục 1
app.MapPost("/api/chat", async (ChatRequest request, FallbackChainService fallback) =>
{
    try
    {
        var response = await fallback.GetChatCompletionAsync(request.Messages);
        return Results.Ok(response);
    }
    catch (AllProvidersFailedException)
    {
        // Response lỗi theo docs/03-API-SPEC.md mục 1: 503 Service Unavailable
        return Results.Json(
            new ChatResponse { Success = false, Error = "Hệ thống đang quá tải, vui lòng thử lại sau ít phút." },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();

static GroqProviderService CreateGroq(IServiceProvider sp, string modelId) =>
    new(sp.GetRequiredService<IHttpClientFactory>().CreateClient("groq"),
        sp.GetRequiredService<IConfiguration>(), modelId);

static GeminiProviderService CreateGemini(IServiceProvider sp, string modelId) =>
    new(sp.GetRequiredService<IHttpClientFactory>().CreateClient("gemini"),
        sp.GetRequiredService<IConfiguration>(), modelId);