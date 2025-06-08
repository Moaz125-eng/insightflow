using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Qa;

public sealed class QuestionAnsweringService : IQuestionAnsweringService
{
    private readonly RagRetriever _retriever;
    private readonly PromptAssembler _assembler = new();
    private readonly LocalInferencePipeline _inference = new();

    public QuestionAnsweringService(RagRetriever retriever)
    {
        _retriever = retriever;
    }

    public async Task<QuestionAnswer> AskAsync(QuestionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            throw new InvalidOperationException("Question is required.");

        var contexts = await _retriever.RetrieveAsync(request, cancellationToken);
        var prompt = _assembler.Build(request.Question, contexts);
        var (answer, confidence) = _inference.Generate(prompt);

        var sources = contexts
            .Select(c => $"doc:{c.DocumentId:N}#chunk:{c.ChunkIndex} score:{c.Score:F3}")
            .ToList();

        return new QuestionAnswer
        {
            Question = request.Question,
            Answer = answer,
            Sources = sources,
            Confidence = confidence,
            AnsweredAt = DateTimeOffset.UtcNow
        };
    }
}
