namespace Application.Interfaces;

public interface IDocumentLoader
{
    Task<string> ExtractTextAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
}
