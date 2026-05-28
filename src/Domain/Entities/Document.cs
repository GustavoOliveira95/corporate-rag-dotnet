using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class Document
{
    public DocumentId Id { get; private set; } = null!;
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public DateTime IngestedAt { get; private set; }
    public int ChunkCount { get; private set; }

    private Document() { }

    public static Document Create(string fileName, string contentType, long fileSizeBytes) =>
        new()
        {
            Id = DocumentId.New(),
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            IngestedAt = DateTime.UtcNow,
            ChunkCount = 0
        };

    /// <summary>Reconstructs a Document from persisted state.</summary>
    public static Document Reconstitute(
        Guid id,
        string fileName,
        string contentType,
        long fileSizeBytes,
        DateTime ingestedAt,
        int chunkCount) =>
        new()
        {
            Id = DocumentId.From(id),
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            IngestedAt = ingestedAt,
            ChunkCount = chunkCount
        };

    public void SetChunkCount(int count) => ChunkCount = count;
}
