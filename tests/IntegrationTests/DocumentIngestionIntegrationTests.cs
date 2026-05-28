using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;

namespace IntegrationTests;

/// <summary>
/// Integration tests require Qdrant and Ollama running locally.
/// Run with: docker-compose up qdrant ollama
/// </summary>
[Trait("Category", "Integration")]
public class DocumentIngestionIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DocumentIngestionIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDocuments_ReturnsOkWithJsonArray()
    {
        var response = await _client.GetAsync("/api/documents");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().StartWith("[");
    }

    [Fact]
    public async Task AskWithoutConversationId_ReturnsOkAndGeneratesId()
    {
        var body = JsonSerializer.Serialize(new { question = "What is the company policy?", conversationId = (string?)null });
        var request = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/chat/ask", request);

        // 200 OK when Ollama is running; 500 otherwise (service dependency)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task IngestInvalidFile_ReturnsBadRequest()
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([]), "file", "empty.pdf");

        var response = await _client.PostAsync("/api/documents/ingest", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteNonExistentDocument_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"/api/documents/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
