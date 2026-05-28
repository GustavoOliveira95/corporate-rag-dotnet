using Application.Interfaces;
using Application.UseCases.AskQuestion;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class AskQuestionHandlerTests
{
    private readonly Mock<IEmbeddingService> _embeddingService = new();
    private readonly Mock<IVectorStoreRepository> _vectorStore = new();
    private readonly Mock<IChatService> _chatService = new();

    private AskQuestionHandler CreateHandler() =>
        new(_embeddingService.Object, _vectorStore.Object, _chatService.Object);

    [Fact]
    public async Task Handle_WithRelevantChunks_BuildsContextAndReturnsAnswer()
    {
        var embedding = new float[3072];
        var chunks = new List<ChunkSearchResult>
        {
            new("doc-1", "Policy.pdf", "The vacation policy allows 20 days per year.", 0, 0.95),
            new("doc-1", "Policy.pdf", "Unused days cannot be carried over.", 1, 0.88)
        };

        _embeddingService.Setup(x => x.GenerateAsync("What is the vacation policy?", It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedding);
        _vectorStore.Setup(x => x.SearchAsync(embedding, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chunks);
        _chatService.Setup(x => x.AskAsync(
                "What is the vacation policy?",
                It.Is<string>(ctx => ctx.Contains("Policy.pdf")),
                "session-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("The vacation policy allows 20 days per year.");

        var result = await CreateHandler().Handle(
            new AskQuestionQuery("What is the vacation policy?", "session-1"),
            CancellationToken.None);

        result.Answer.Should().Be("The vacation policy allows 20 days per year.");
        result.ConversationId.Should().Be("session-1");
    }

    [Fact]
    public async Task Handle_NoChunksFound_PassesNoContextMessage()
    {
        _embeddingService.Setup(x => x.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new float[3072]);
        _vectorStore.Setup(x => x.SearchAsync(It.IsAny<float[]>(), 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChunkSearchResult>());
        _chatService.Setup(x => x.AskAsync(
                It.IsAny<string>(),
                "No relevant documents found.",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("I don't have information about that in the provided documents.");

        var result = await CreateHandler().Handle(
            new AskQuestionQuery("Random unknown topic", "session-2"),
            CancellationToken.None);

        result.Answer.Should().Contain("don't have information");
    }

    [Fact]
    public async Task Handle_PreservesConversationId()
    {
        _embeddingService.Setup(x => x.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new float[3072]);
        _vectorStore.Setup(x => x.SearchAsync(It.IsAny<float[]>(), 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChunkSearchResult>());
        _chatService.Setup(x => x.AskAsync(It.IsAny<string>(), It.IsAny<string>(), "my-session", It.IsAny<CancellationToken>()))
            .ReturnsAsync("Answer");

        var result = await CreateHandler().Handle(
            new AskQuestionQuery("Question", "my-session"),
            CancellationToken.None);

        result.ConversationId.Should().Be("my-session");
    }
}
