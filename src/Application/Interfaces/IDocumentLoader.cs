namespace Application.Interfaces;

/// <summary>Extracts plain text from a document byte stream.</summary>
public interface IDocumentLoader
{
    Task<string> ExtractTextAsync(Stream stream, CancellationToken cancellationToken = default);
}
