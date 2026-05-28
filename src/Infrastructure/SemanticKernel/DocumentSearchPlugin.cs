using Application.Interfaces;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Infrastructure.SemanticKernel;

public sealed class DocumentSearchPlugin
{
    private readonly IVectorStoreRepository _vectorStore;
    private readonly IEmbeddingService _embeddingService;

    public DocumentSearchPlugin(IVectorStoreRepository vectorStore, IEmbeddingService embeddingService)
    {
        _vectorStore = vectorStore;
        _embeddingService = embeddingService;
    }

    [KernelFunction("search_documents")]
    [Description("Search for relevant document chunks based on a semantic query against the corporate document store")]
    public async Task<string> SearchDocumentsAsync(
        [Description("The search query to find relevant information")] string query,
        CancellationToken cancellationToken = default)
    {
        var embedding = await _embeddingService.GenerateAsync(query, cancellationToken);
        var results = await _vectorStore.SearchAsync(embedding, topK: 5, cancellationToken);

        if (results.Count == 0)
            return "No relevant documents found.";

        return string.Join(
            "\n\n---\n\n",
            results.Select(r => $"[Source: {r.DocumentName}, Chunk {r.ChunkIndex}]\n{r.Content}"));
    }
}
