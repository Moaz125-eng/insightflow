using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightFlow.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
[Authorize(Policy = "AdminOnly")]
public sealed class WebhooksController : ControllerBase
{
    private readonly IWebhookService _webhookService;

    public WebhooksController(IWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    [HttpPost]
    public async Task<ActionResult<WebhookResponse>> Register([FromBody] RegisterWebhookRequest request, CancellationToken cancellationToken)
    {
        var subscription = await _webhookService.RegisterAsync(request.TargetUrl, request.EventType, cancellationToken);
        return CreatedAtAction(nameof(List), WebhookResponse.From(subscription));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WebhookResponse>>> List(CancellationToken cancellationToken)
    {
        var items = await _webhookService.ListAsync(cancellationToken);
        return Ok(items.Select(WebhookResponse.From).ToList());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _webhookService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}

public sealed record RegisterWebhookRequest(string TargetUrl, string EventType);

public sealed record WebhookResponse(Guid Id, string TargetUrl, string EventType, bool IsActive)
{
    public static WebhookResponse From(WebhookSubscription subscription) =>
        new(subscription.Id, subscription.TargetUrl, subscription.EventType, subscription.IsActive);
}
