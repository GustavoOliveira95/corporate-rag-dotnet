using MediatR;

namespace Application.UseCases.IngestDocument;

public record IngestDocumentCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSizeBytes) : IRequest<IngestDocumentResult>;

public record IngestDocumentResult(Guid DocumentId, int ChunkCount);
