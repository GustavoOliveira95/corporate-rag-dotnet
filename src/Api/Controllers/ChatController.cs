using Application.UseCases.AskQuestion;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public record AskRequest(string Question, string? ConversationId);

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator) => _mediator = mediator;

    /// <summary>Ask a question. A conversation ID is generated automatically when omitted.</summary>
    [HttpPost("ask")]
    [ProducesResponseType(typeof(AskQuestionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ask([FromBody] AskRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { error = "Question must not be empty." });

        var conversationId = string.IsNullOrWhiteSpace(request.ConversationId)
            ? Guid.NewGuid().ToString()
            : request.ConversationId;

        var result = await _mediator.Send(new AskQuestionQuery(request.Question, conversationId), cancellationToken);
        return Ok(result);
    }
}
