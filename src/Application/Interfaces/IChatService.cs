namespace Application.Interfaces;

/// <summary>Generates answers from an LLM using retrieved context and conversation history.</summary>
public interface IChatService
{
    Task<string> AskAsync(
        string question,
        string context,
        string conversationId,
        CancellationToken cancellationToken = default);
}
