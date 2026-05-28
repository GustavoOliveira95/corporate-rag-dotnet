using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.VectorData;
using Qdrant.Client;
using Qdrant.Client.Grpc;

#pragma warning disable SKEXP0001  // IVectorStore is experimental

namespace Infrastructure.VectorStore;

public sealed class QdrantVectorStoreRepository : IVectorStoreRepository
{
    private const string CollectionName = "document_chunks";

    private readonly IVectorStore _vectorStore;
    private readonly QdrantClient _qdrantClient;

    public QdrantVectorStoreRepository(IVectorStore vectorStore, QdrantClient qdrantClient)
    {
        _vectorStore = vectorStore;
        _qdrantClient = qdrantClient;
    }

    public async Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();
        await collection.CreateCollectionIfNotExistsAsync(cancellationToken);
    }

    public async Task UpsertBatchAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        foreach (var chunk in chunks)
        {
            var record = new DocumentChunkRecord
            {
                Id = chunk.Id,
                DocumentId = chunk.DocumentId.Value.ToString(),
                DocumentName = chunk.DocumentName,
                Content = chunk.Content.Value,
                ChunkIndex = chunk.ChunkIndex,
                Embedding = new ReadOnlyMemory<float>(chunk.Embedding)
            };

            await collection.UpsertAsync(record, cancellationToken: cancellationToken);
        }
    }

    public async Task<IReadOnlyList<ChunkSearchResult>> SearchAsync(
        float[] embedding,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();
        var memory = new ReadOnlyMemory<float>(embedding);
        var options = new VectorSearchOptions { Top = topK };
        var results = new List<ChunkSearchResult>();

        var searchResults = await collection.VectorizedSearchAsync(memory, options, cancellationToken);

        await foreach (var result in searchResults.Results)
        {
            results.Add(new ChunkSearchResult(
                result.Record.DocumentId,
                result.Record.DocumentName,
                result.Record.Content,
                result.Record.ChunkIndex,
                result.Score ?? 0));
        }

        return results;
    }

    public async Task DeleteByDocumentIdAsync(string documentId, CancellationToken cancellationToken = default)
    {
        var filter = new Filter();
        filter.Must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = "DocumentId",
                Match = new Match { Text = documentId }
            }
        });

        await _qdrantClient.DeleteAsync(CollectionName, filter, cancellationToken: cancellationToken);
    }

    private IVectorStoreRecordCollection<Guid, DocumentChunkRecord> GetCollection()
        => _vectorStore.GetCollection<Guid, DocumentChunkRecord>(CollectionName);
}
