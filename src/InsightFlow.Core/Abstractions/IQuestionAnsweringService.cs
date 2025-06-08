using InsightFlow.Core.Models;

namespace InsightFlow.Core.Abstractions;

public interface IQuestionAnsweringService
{
    Task<QuestionAnswer> AskAsync(QuestionRequest request, CancellationToken cancellationToken = default);
}
