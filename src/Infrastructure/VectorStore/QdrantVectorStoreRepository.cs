using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.VectorData;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using VDStore = Microsoft.Extensions.VectorData.VectorStore;

namespace Infrastructure.VectorStore;

public sealed class QdrantVectorStoreRepository : IVectorStoreRepository
{
    private const string CollectionName = "document_chunks";

    private readonly VDStore _vectorStore;
    private readonly QdrantClient _qdrantClient;

    public QdrantVectorStoreRepository(VDStore vectorStore, QdrantClient qdrantClient)
    {
        _vectorStore = vectorStore;
        _qdrantClient = qdrantClient;
    }

    public async Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();
        await collection.EnsureCollectionExistsAsync(cancellationToken);
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
        var options = new VectorSearchOptions<DocumentChunkRecord>();
        var results = new List<ChunkSearchResult>();

        var searchResults = collection.SearchAsync(memory, topK, options, cancellationToken);

        await foreach (var result in searchResults)
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

    private VectorStoreCollection<Guid, DocumentChunkRecord> GetCollection()
        => (VectorStoreCollection<Guid, DocumentChunkRecord>)_vectorStore.GetCollection<Guid, DocumentChunkRecord>(CollectionName);
}
