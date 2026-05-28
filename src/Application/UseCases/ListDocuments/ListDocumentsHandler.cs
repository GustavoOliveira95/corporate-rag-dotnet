using Application.Interfaces;
using MediatR;

namespace Application.UseCases.ListDocuments;

public sealed class ListDocumentsHandler : IRequestHandler<ListDocumentsQuery, IReadOnlyList<DocumentDto>>
{
    private readonly IDocumentRepository _repository;

    public ListDocumentsHandler(IDocumentRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<DocumentDto>> Handle(ListDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _repository.GetAllAsync(cancellationToken);
        return documents
            .Select(d => new DocumentDto(d.Id.Value, d.FileName, d.ContentType, d.FileSizeBytes, d.IngestedAt, d.ChunkCount))
            .ToList();
    }
}
