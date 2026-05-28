using Application.Common;
using Application.Interfaces;
using Infrastructure.DocumentLoader;
using Infrastructure.EmbeddingService;
using Infrastructure.LLMService;
using Infrastructure.Persistence;
using Infrastructure.SemanticKernel;
using Infrastructure.VectorStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

#pragma warning disable SKEXP0070  // Ollama connector is experimental
#pragma warning disable SKEXP0020  // Qdrant VectorStore connector is experimental

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var ollamaEndpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
        var qdrantHost = configuration["Qdrant:Host"] ?? "localhost";
        var qdrantPort = int.Parse(configuration["Qdrant:Port"] ?? "6333");
        var dataPath = configuration["DataPath"];

        if (string.IsNullOrEmpty(dataPath))
            dataPath = Path.Combine(AppContext.BaseDirectory, "data");

        // Semantic Kernel with Ollama
        var kernelBuilder = services.AddKernel();
        kernelBuilder.AddOllamaTextEmbeddingGeneration("phi3", new Uri(ollamaEndpoint));
        kernelBuilder.AddOllamaChatCompletion("phi3", new Uri(ollamaEndpoint));

        // Qdrant: register client first so both the vector store and the repository share it
        services.AddSingleton(new QdrantClient(qdrantHost, qdrantPort));
        services.AddQdrantVectorStore();

        // Application services
        services.AddSingleton<ChunkingService>();
        services.AddSingleton<IConversationHistoryService, InMemoryConversationHistoryService>();
        services.AddSingleton<IDocumentLoader, PdfDocumentLoader>();
        services.AddSingleton<IDocumentRepository>(_ =>
            new JsonFileDocumentRepository(Path.Combine(dataPath, "documents.json")));

        services.AddScoped<IEmbeddingService, OllamaEmbeddingService>();
        services.AddScoped<IChatService, OllamaChatService>();
        services.AddScoped<IVectorStoreRepository, QdrantVectorStoreRepository>();
        services.AddScoped<DocumentSearchPlugin>();

        return services;
    }
}
