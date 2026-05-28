using Application.Interfaces;
using MediatR;

namespace Application.UseCases.DeleteDocument;

public sealed class DeleteDocumentHandler : IRequestHandler<DeleteDocumentCommand, bool>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IVectorStoreRepository _vectorStore;

    public DeleteDocumentHandler(IDocumentRepository documentRepository, IVectorStoreRepository vectorStore)
    {
        _documentRepository = documentRepository;
        _vectorStore = vectorStore;
    }

    public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null) return false;

        await _vectorStore.DeleteByDocumentIdAsync(request.DocumentId.ToString(), cancellationToken);
        await _documentRepository.DeleteAsync(request.DocumentId, cancellationToken);
        return true;
    }
}
