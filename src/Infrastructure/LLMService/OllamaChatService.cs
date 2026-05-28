using Application.Interfaces;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Infrastructure.LLMService;

public sealed class OllamaChatService : IChatService
{
    private const string SystemPromptTemplate =
        """
        You are a corporate assistant. Answer ONLY based on the following context extracted from internal company documents.
        If the answer is not in the context, say exactly: "I don't have information about that in the provided documents."
        Do not make up information. Be concise and professional.

        Context:
        {0}
        """;

    private readonly IChatCompletionService _chatCompletion;
    private readonly IConversationHistoryService _historyService;

    public OllamaChatService(IChatCompletionService chatCompletion, IConversationHistoryService historyService)
    {
        _chatCompletion = chatCompletion;
        _historyService = historyService;
    }

    public async Task<string> AskAsync(
        string question,
        string context,
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(string.Format(SystemPromptTemplate, context));

        foreach (var msg in _historyService.GetHistory(conversationId))
        {
            if (msg.Role == "user")
                chatHistory.AddUserMessage(msg.Content);
            else
                chatHistory.AddAssistantMessage(msg.Content);
        }

        chatHistory.AddUserMessage(question);

        var response = await _chatCompletion.GetChatMessageContentAsync(
            chatHistory,
            cancellationToken: cancellationToken);

        var answer = response.Content ?? string.Empty;

        _historyService.AddMessage(conversationId, new ConversationMessage("user", question));
        _historyService.AddMessage(conversationId, new ConversationMessage("assistant", answer));

        return answer;
    }
}
