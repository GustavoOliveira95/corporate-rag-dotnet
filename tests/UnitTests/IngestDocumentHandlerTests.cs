using Application.Common;
using Application.Interfaces;
using Application.UseCases.IngestDocument;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class IngestDocumentHandlerTests
{
    private readonly Mock<IDocumentLoader> _documentLoader = new();
    private readonly Mock<IEmbeddingService> _embeddingService = new();
    private readonly Mock<IVectorStoreRepository> _vectorStore = new();
    private readonly Mock<IDocumentRepository> _documentRepository = new();
    private readonly ChunkingService _chunkingService = new();

    private IngestDocumentHandler CreateHandler() => new(
        _documentLoader.Object,
        _embeddingService.Object,
        _vectorStore.Object,
        _documentRepository.Object,
        _chunkingService);

    [Fact]
    public async Task Handle_ValidDocument_ReturnsDocumentIdAndChunkCount()
    {
        var text = string.Join(' ', Enumerable.Repeat("word", 100));
        _documentLoader.Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(text);
        _embeddingService.Setup(x => x.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new float[3072]);

        var command = new IngestDocumentCommand(Stream.Null, "policy.pdf", "application/pdf", 2048);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.DocumentId.Should().NotBeEmpty();
        result.ChunkCount.Should().Be(1);
        _vectorStore.Verify(x => x.UpsertBatchAsync(It.IsAny<IEnumerable<DocumentChunk>>(), It.IsAny<CancellationToken>()), Times.Once);
        _documentRepository.Verify(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LargeDocument_ProducesMultipleChunks()
    {
        var text = string.Join(' ', Enumerable.Repeat("word", 2000));
        _documentLoader.Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(text);
        _embeddingService.Setup(x => x.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new float[3072]);

        var command = new IngestDocumentCommand(Stream.Null, "large.pdf", "application/pdf", 10240);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.ChunkCount.Should().BeGreaterThan(1);
        _embeddingService.Verify(x => x.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(result.ChunkCount));
    }

    [Fact]
    public async Task Handle_AssignsUniqueDocumentIdPerCall()
    {
        var text = string.Join(' ', Enumerable.Repeat("word", 10));
        _documentLoader.Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(text);
        _embeddingService.Setup(x => x.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new float[3072]);

        var command = new IngestDocumentCommand(Stream.Null, "doc.pdf", "application/pdf", 512);
        var result1 = await CreateHandler().Handle(command, CancellationToken.None);
        var result2 = await CreateHandler().Handle(command, CancellationToken.None);

        result1.DocumentId.Should().NotBe(result2.DocumentId);
    }
}
