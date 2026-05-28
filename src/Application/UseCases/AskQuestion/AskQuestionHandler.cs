using Application.Interfaces;
using MediatR;

namespace Application.UseCases.AskQuestion;

public sealed class AskQuestionHandler : IRequestHandler<AskQuestionQuery, AskQuestionResult>
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStoreRepository _vectorStore;
    private readonly IChatService _chatService;

    public AskQuestionHandler(
        IEmbeddingService embeddingService,
        IVectorStoreRepository vectorStore,
        IChatService chatService)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _chatService = chatService;
    }

    public async Task<AskQuestionResult> Handle(AskQuestionQuery request, CancellationToken cancellationToken)
    {
        var embedding = await _embeddingService.GenerateAsync(request.Question, cancellationToken);
        var chunks = await _vectorStore.SearchAsync(embedding, topK: 5, cancellationToken);

        var context = chunks.Count > 0
            ? string.Join("\n\n---\n\n", chunks.Select(c => $"[{c.DocumentName}]\n{c.Content}"))
            : "No relevant documents found.";

        var answer = await _chatService.AskAsync(request.Question, context, request.ConversationId, cancellationToken);
        return new AskQuestionResult(answer, request.ConversationId);
    }
}
