using Microsoft.Extensions.VectorData;

namespace Infrastructure.VectorStore;

public sealed class DocumentChunkRecord
{
    [VectorStoreKey]
    public Guid Id { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public string DocumentId { get; set; } = string.Empty;

    [VectorStoreData(IsIndexed = true)]
    public string DocumentName { get; set; } = string.Empty;

    [VectorStoreData]
    public string Content { get; set; } = string.Empty;

    [VectorStoreData]
    public int ChunkIndex { get; set; }

    [VectorStoreVector(2048, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float>? Embedding { get; set; }
}
