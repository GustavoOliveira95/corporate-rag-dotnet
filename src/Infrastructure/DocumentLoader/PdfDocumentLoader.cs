using Application.Interfaces;
using System.Text;
using UglyToad.PdfPig;

namespace Infrastructure.DocumentLoader;

public sealed class PdfDocumentLoader : IDocumentLoader
{
    public async Task<string> ExtractTextAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);

        using var document = PdfDocument.Open(ms.ToArray());
        var sb = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            sb.AppendLine(page.Text);
        }

        return sb.ToString();
    }
}
