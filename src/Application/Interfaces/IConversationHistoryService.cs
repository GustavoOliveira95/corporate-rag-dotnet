namespace Application.Interfaces;

public record ConversationMessage(string Role, string Content);

/// <summary>Maintains in-memory conversation history keyed by conversation ID.</summary>
public interface IConversationHistoryService
{
    IReadOnlyList<ConversationMessage> GetHistory(string conversationId);
    void AddMessage(string conversationId, ConversationMessage message);
    void Clear(string conversationId);
}
