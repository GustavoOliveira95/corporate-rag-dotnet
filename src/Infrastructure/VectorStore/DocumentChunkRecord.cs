using Microsoft.Extensions.VectorData;

namespace Infrastructure.VectorStore;

public sealed class DocumentChunkRecord
{
    [VectorStoreRecordKey]
    public Guid Id { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    public string DocumentId { get; set; } = string.Empty;

    [VectorStoreRecordData(IsFilterable = true)]
    public string DocumentName { get; set; } = string.Empty;

    [VectorStoreRecordData]
    public string Content { get; set; } = string.Empty;

    [VectorStoreRecordData]
    public int ChunkIndex { get; set; }

    [VectorStoreRecordVector(3072, DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float>? Embedding { get; set; }
}
