using MediatR;

namespace Application.UseCases.DeleteDocument;

public record DeleteDocumentCommand(Guid DocumentId) : IRequest<bool>;
