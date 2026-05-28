namespace Application.Interfaces;

/// <summary>Generates dense vector embeddings for a text input.</summary>
public interface IEmbeddingService
{
    Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken = default);
}
