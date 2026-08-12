using System.Text.Json.Serialization;

namespace Backend.Models;

// Dùng chung 1 class cho cả response thành công lẫn response lỗi:
// các field success (type/content/providerUsed) và error đều nullable,
// JsonIgnore WhenWritingNull giúp JSON output khớp đúng shape của spec
// (response lỗi chỉ có success=false + error, không kèm field thừa).
public class ChatResponse
{
    public bool Success { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Content { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ProviderUsed { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }
}