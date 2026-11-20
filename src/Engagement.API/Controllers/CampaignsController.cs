using Engagement.API.Domain.Entities;
using Engagement.API.Domain.Events;
using Engagement.API.Infrastructure.Messaging;
using Engagement.API.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Engagement.API.Controllers;

[ApiController]
[Route("api/v1/campaigns")]
public sealed class CampaignsController(
    ICampaignRepository repository,
    IKafkaEventProducer kafkaProducer,
    IOptions<KafkaSettings> kafkaOptions) : ControllerBase
{
    [HttpGet("tenant/{tenantId}")]
    public async Task<IActionResult> GetActiveWidgets(string tenantId, CancellationToken ct)
    {
        var campaigns = await repository.GetActiveByTenantAsync(tenantId, ct);
        return Ok(new
        {
            tenantId,
            count = campaigns.Count,
            widgets = campaigns
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateCampaign([FromBody] WidgetCampaign request, CancellationToken ct)
    {
        var created = await repository.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var campaign = await repository.GetByIdAsync(id, ct);
        return campaign is not null ? Ok(campaign) : NotFound();
    }

    [HttpPut("{id}/layout")]
    public async Task<IActionResult> UpdateDnDLayout(string id, [FromBody] WidgetLayoutConfig layoutConfig, CancellationToken ct)
    {
        var updated = await repository.UpdateLayoutAsync(id, layoutConfig, ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id}/telemetry")]
    public async Task<IActionResult> TrackInteraction(
        string id,
        [FromQuery] string tenantId,
        [FromQuery] string interactionType,
        [FromQuery] string? productSku,
        CancellationToken ct)
    {
        var evt = new WidgetInteractionEvent(
            EventId: Guid.NewGuid().ToString("N"),
            TenantId: tenantId,
            CampaignId: id,
            WidgetType: "StorefrontWidget",
            InteractionType: interactionType,
            ProductSku: productSku,
            SessionId: Request.Headers["X-Storefront-Session"].FirstOrDefault(),
            OccurredAtUtc: DateTime.UtcNow
        );

        await kafkaProducer.PublishAsync(kafkaOptions.Value.InteractionTopic, tenantId, evt, ct);
        return Accepted(evt);
    }
}
