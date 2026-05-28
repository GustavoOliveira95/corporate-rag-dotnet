using Application.Interfaces;
using System.Collections.Concurrent;

namespace Infrastructure.LLMService;

public sealed class InMemoryConversationHistoryService : IConversationHistoryService
{
    private readonly ConcurrentDictionary<string, List<ConversationMessage>> _sessions = new();

    public IReadOnlyList<ConversationMessage> GetHistory(string conversationId)
        => _sessions.GetOrAdd(conversationId, _ => []).AsReadOnly();

    public void AddMessage(string conversationId, ConversationMessage message)
        => _sessions.GetOrAdd(conversationId, _ => []).Add(message);

    public void Clear(string conversationId)
        => _sessions.TryRemove(conversationId, out _);
}
