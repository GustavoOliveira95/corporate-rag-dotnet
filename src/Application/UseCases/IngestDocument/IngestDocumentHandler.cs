using Application.Common;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.UseCases.IngestDocument;

public sealed class IngestDocumentHandler : IRequestHandler<IngestDocumentCommand, IngestDocumentResult>
{
    private readonly IDocumentLoader _documentLoader;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStoreRepository _vectorStore;
    private readonly IDocumentRepository _documentRepository;
    private readonly ChunkingService _chunkingService;

    public IngestDocumentHandler(
        IDocumentLoader documentLoader,
        IEmbeddingService embeddingService,
        IVectorStoreRepository vectorStore,
        IDocumentRepository documentRepository,
        ChunkingService chunkingService)
    {
        _documentLoader = documentLoader;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _documentRepository = documentRepository;
        _chunkingService = chunkingService;
    }

    public async Task<IngestDocumentResult> Handle(IngestDocumentCommand request, CancellationToken cancellationToken)
    {
        var text = await _documentLoader.ExtractTextAsync(request.FileStream, request.FileName, cancellationToken);
        var chunks = _chunkingService.Chunk(text).ToList();
        var document = Document.Create(request.FileName, request.ContentType, request.FileSizeBytes);

        var chunkEntities = new List<DocumentChunk>(chunks.Count);
        for (var i = 0; i < chunks.Count; i++)
        {
            var embedding = await _embeddingService.GenerateAsync(chunks[i], cancellationToken);
            chunkEntities.Add(DocumentChunk.Create(document.Id, document.FileName, chunks[i], i, embedding));
        }

        await _vectorStore.UpsertBatchAsync(chunkEntities, cancellationToken);
        document.SetChunkCount(chunks.Count);
        await _documentRepository.AddAsync(document, cancellationToken);

        return new IngestDocumentResult(document.Id.Value, chunks.Count);
    }
}
