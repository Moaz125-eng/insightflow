using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace InsightFlow.Api.Controllers;

[ApiController]
[Route("api/qa")]
public sealed class QaController : ControllerBase
{
    private readonly IQuestionAnsweringService _questionService;

    public QaController(IQuestionAnsweringService questionService)
    {
        _questionService = questionService;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<QaResponse>> Ask([FromBody] QaRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { error = "Question is required." });

        var answer = await _questionService.AskAsync(
            new QuestionRequest
            {
                Question = request.Question,
                DocumentId = request.DocumentId,
                ContextChunks = request.ContextChunks ?? 5
            },
            cancellationToken);

        return Ok(QaResponse.From(answer));
    }
}

public sealed record QaRequest(string Question, Guid? DocumentId, int? ContextChunks);

public sealed record QaResponse(string Question, string Answer, float Confidence, IReadOnlyList<string> Sources)
{
    public static QaResponse From(QuestionAnswer answer) =>
        new(answer.Question, answer.Answer, answer.Confidence, answer.Sources);
}
