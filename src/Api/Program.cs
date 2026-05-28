using Application.Interfaces;
using Application.UseCases.IngestDocument;
using Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Corporate RAG API",
        Version = "v1",
        Description = "Corporate chatbot powered by RAG — answers questions based on internal company documents."
    });
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(IngestDocumentHandler).Assembly));

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Ensure the Qdrant collection exists before the first request
using (var scope = app.Services.CreateScope())
{
    var vectorStore = scope.ServiceProvider.GetRequiredService<IVectorStoreRepository>();
    await vectorStore.EnsureCollectionExistsAsync();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Corporate RAG API v1"));

app.MapControllers();
app.Run();

// Expose Program for integration tests
public partial class Program { }
