using Domain.Entities;

namespace Application.Interfaces;

public record ChunkSearchResult(
    string DocumentId,
    string DocumentName,
    string Content,
    int ChunkIndex,
    double Score);

/// <summary>Abstracts vector store operations for document chunks.</summary>
public interface IVectorStoreRepository
{
    /// <summary>Ensures the backing collection exists before first use.</summary>
    Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default);

    /// <summary>Upserts a batch of embedded chunks.</summary>
    Task UpsertBatchAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default);

    /// <summary>Returns the top-K chunks most semantically similar to the query embedding.</summary>
    Task<IReadOnlyList<ChunkSearchResult>> SearchAsync(float[] embedding, int topK = 5, CancellationToken cancellationToken = default);

    /// <summary>Deletes all chunks belonging to a document.</summary>
    Task DeleteByDocumentIdAsync(string documentId, CancellationToken cancellationToken = default);
}
