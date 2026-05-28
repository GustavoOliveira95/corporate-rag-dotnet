using MediatR;

namespace Application.UseCases.ListDocuments;

public record ListDocumentsQuery : IRequest<IReadOnlyList<DocumentDto>>;

public record DocumentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    DateTime IngestedAt,
    int ChunkCount);
