using MediatR;

namespace Application.UseCases.AskQuestion;

public record AskQuestionQuery(string Question, string ConversationId) : IRequest<AskQuestionResult>;

public record AskQuestionResult(string Answer, string ConversationId);
