namespace Backend.Models;

public class ChatRequest
{
    public List<MessageDto> Messages { get; set; } = new();
}