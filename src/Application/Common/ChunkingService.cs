namespace Application.Common;

public sealed class ChunkingService
{
    /// <summary>
    /// Splits text into overlapping word-based chunks.
    /// </summary>
    public IEnumerable<string> Chunk(string text, int chunkSize = 500, int overlap = 50)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) yield break;

        var step = chunkSize - overlap;

        for (var i = 0; i < words.Length; i += step)
        {
            yield return string.Join(' ', words.Skip(i).Take(chunkSize));

            if (i + chunkSize >= words.Length)
                break;
        }
    }
}
