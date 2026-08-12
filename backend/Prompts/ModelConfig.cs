namespace Backend.Prompts;

/// <summary>
/// Đây là NƠI DUY NHẤT chứa model ID của mọi AI Provider.
/// Khi nhà cung cấp deprecate model, chỉ sửa model ID tại file này,
/// KHÔNG được hardcode model ID ở bất kỳ file nào khác.
/// Thứ tự khai báo khớp đúng Fallback Chain ở docs/03-API-SPEC.md mục 2.
/// </summary>
public static class ModelConfig
{
    public const string Groq_Primary = "qwen/qwen3.6-27b";
    public const string Groq_Fallback = "openai/gpt-oss-120b";
    public const string Gemini_Primary = "gemini-3.6-flash";
    public const string Gemini_Fallback = "gemini-3.5-flash-lite";
}