namespace Backend.Services;

/// <summary>
/// Ném ra khi TẤT CẢ các AI Provider trong Fallback Chain đều thất bại.
/// Program.cs sẽ bắt exception này để trả lỗi thân thiện cho Client (Phase 2).
/// </summary>
public class AllProvidersFailedException : Exception
{
    public AllProvidersFailedException() { }

    public AllProvidersFailedException(string message) : base(message) { }

    public AllProvidersFailedException(string message, Exception inner) : base(message, inner) { }
}