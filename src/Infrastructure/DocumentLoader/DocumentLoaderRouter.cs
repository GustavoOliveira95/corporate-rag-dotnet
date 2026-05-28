using Application.Interfaces;

namespace Infrastructure.DocumentLoader;

public sealed class DocumentLoaderRouter : IDocumentLoader
{
    private readonly PdfDocumentLoader _pdfLoader;
    private readonly CsvDocumentLoader _csvLoader;

    public DocumentLoaderRouter(PdfDocumentLoader pdfLoader, CsvDocumentLoader csvLoader)
    {
        _pdfLoader = pdfLoader;
        _csvLoader = csvLoader;
    }

    public Task<string> ExtractTextAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => _pdfLoader.ExtractTextAsync(stream, fileName, cancellationToken),
            ".csv" => _csvLoader.ExtractTextAsync(stream, fileName, cancellationToken),
            _ => throw new NotSupportedException($"File type '{ext}' is not supported. Supported types: .pdf, .csv")
        };
    }
}
