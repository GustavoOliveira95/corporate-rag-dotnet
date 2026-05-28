using Application.Interfaces;
using Microsoft.SemanticKernel.Embeddings;

namespace Infrastructure.EmbeddingService;

public sealed class OllamaEmbeddingService : IEmbeddingService
{
    private readonly ITextEmbeddingGenerationService _embeddingService;

    public OllamaEmbeddingService(ITextEmbeddingGenerationService embeddingService)
        => _embeddingService = embeddingService;

    public async Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken = default)
    {
        var embeddings = await _embeddingService.GenerateEmbeddingsAsync(
            [text],
            cancellationToken: cancellationToken);

        return embeddings[0].ToArray();
    }
}
