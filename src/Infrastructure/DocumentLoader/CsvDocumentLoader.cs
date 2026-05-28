using Application.Interfaces;
using System.Text;

namespace Infrastructure.DocumentLoader;

public sealed class CsvDocumentLoader : IDocumentLoader
{
    public async Task<string> ExtractTextAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var content = await reader.ReadToEndAsync(cancellationToken);

        var lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
            return string.Empty;

        var headers = ParseLine(lines[0]);
        var sb = new StringBuilder();

        for (var i = 1; i < lines.Length; i++)
        {
            var values = ParseLine(lines[i]);
            var parts = new List<string>(headers.Length);

            for (var j = 0; j < headers.Length; j++)
            {
                var value = j < values.Length ? values[j].Trim() : string.Empty;
                if (!string.IsNullOrEmpty(value))
                    parts.Add($"{headers[j].Trim()}: {value}");
            }

            if (parts.Count > 0)
                sb.AppendLine(string.Join(", ", parts));
        }

        return sb.ToString();
    }

    private static string[] ParseLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var ch in line)
        {
            if (ch == '"')
                inQuotes = !inQuotes;
            else if (ch == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
                current.Append(ch);
        }

        fields.Add(current.ToString());
        return [.. fields];
    }
}
