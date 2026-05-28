using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class DocumentChunk
{
    public Guid Id { get; private set; }
    public DocumentId DocumentId { get; private set; } = null!;
    public string DocumentName { get; private set; } = string.Empty;
    public ChunkContent Content { get; private set; } = null!;
    public int ChunkIndex { get; private set; }
    public float[] Embedding { get; private set; } = [];

    private DocumentChunk() { }

    public static DocumentChunk Create(
        DocumentId documentId,
        string documentName,
        string content,
        int chunkIndex,
        float[] embedding) =>
        new()
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            DocumentName = documentName,
            Content = ChunkContent.From(content),
            ChunkIndex = chunkIndex,
            Embedding = embedding
        };
}
