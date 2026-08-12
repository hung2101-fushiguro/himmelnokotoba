using Backend.Models;

namespace Backend.Services;

public interface IAiProviderService
{
    /// <summary>
    /// Gọi AI Provider để lấy câu trả lời cho toàn bộ lịch sử hội thoại
    /// (đã bao gồm câu hỏi hiện tại ở tin nhắn cuối cùng), cùng với
    /// System Prompt cố định để hướng dẫn cách trả lời.
    /// </summary>
    /// <param name="messages">Toàn bộ lịch sử hội thoại do Client gửi lên.</param>
    /// <param name="systemPrompt">System Prompt lấy từ Prompts/SystemPrompts.cs.</param>
    /// <returns>Chuỗi text thô provider trả về (bao gồm dòng TYPE: ở đầu).</returns>
    /// <remarks>
    /// QUAN TRỌNG: implementation KHÔNG được tự nuốt lỗi (try-catch rồi trả về
    /// text thành công). Khi provider lỗi (429 / 5xx / timeout / network...),
    /// PHẢI throw exception để FallbackChainService và Polly bắt được và rớt
    /// xuống provider tiếp theo trong chain.
    /// </remarks>
    Task<string> GetCompletionAsync(List<MessageDto> messages, string systemPrompt);
}