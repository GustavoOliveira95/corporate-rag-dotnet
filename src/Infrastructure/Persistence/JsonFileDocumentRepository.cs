using Application.Interfaces;
using Domain.Entities;
using System.Text.Json;

namespace Infrastructure.Persistence;

public sealed class JsonFileDocumentRepository : IDocumentRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public JsonFileDocumentRepository(string filePath) => _filePath = filePath;

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var records = await ReadAllAsync();
            records.Add(DocumentRecord.From(document));
            await WriteAllAsync(records);
        }
        finally { _lock.Release(); }
    }

    public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await ReadAllAsync();
        return records.Select(r => r.ToDomain()).ToList();
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var records = await ReadAllAsync();
        return records.FirstOrDefault(r => r.Id == id)?.ToDomain();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var records = await ReadAllAsync();
            records.RemoveAll(r => r.Id == id);
            await WriteAllAsync(records);
        }
        finally { _lock.Release(); }
    }

    private async Task<List<DocumentRecord>> ReadAllAsync()
    {
        if (!File.Exists(_filePath)) return [];
        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<DocumentRecord>>(json) ?? [];
    }

    private async Task WriteAllAsync(List<DocumentRecord> records)
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(records, JsonOptions));
    }

    private sealed record DocumentRecord(
        Guid Id,
        string FileName,
        string ContentType,
        long FileSizeBytes,
        DateTime IngestedAt,
        int ChunkCount)
    {
        public static DocumentRecord From(Document d) =>
            new(d.Id.Value, d.FileName, d.ContentType, d.FileSizeBytes, d.IngestedAt, d.ChunkCount);

        public Document ToDomain() =>
            Document.Reconstitute(Id, FileName, ContentType, FileSizeBytes, IngestedAt, ChunkCount);
    }
}
