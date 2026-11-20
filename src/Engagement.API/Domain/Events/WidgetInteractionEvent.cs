namespace Engagement.API.Domain.Events;

public sealed record WidgetInteractionEvent(
    string EventId,
    string TenantId,
    string CampaignId,
    string WidgetType,
    string InteractionType, // impression | click | add_to_cart | wheel_spin
    string? ProductSku,
    string? SessionId,
    DateTime OccurredAtUtc
);
