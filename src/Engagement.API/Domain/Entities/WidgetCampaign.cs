using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Engagement.API.Domain.Entities;

public enum WidgetType
{
    FlashProducts,
    CartRecommendation,
    SpinTheWheel,
    SizeChartModal
}

public sealed class WidgetCampaign
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("tenantId")]
    public required string TenantId { get; set; }

    [BsonElement("platform")]
    public string Platform { get; set; } = "Universal"; // Shopify, Ikas, Ideasoft, Custom

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("widgetType")]
    [BsonRepresentation(BsonType.String)]
    public WidgetType WidgetType { get; set; }

    [BsonElement("targetDomSelector")]
    public string TargetDomSelector { get; set; } = "#yuddy-widget-root";

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("layoutConfig")]
    public WidgetLayoutConfig LayoutConfig { get; set; } = new();

    [BsonElement("productIds")]
    public List<string> ProductIds { get; set; } = [];

    [BsonElement("wheelSlices")]
    public List<SpinWheelSlice> WheelSlices { get; set; } = [];

    [BsonElement("updatedAtUtc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class WidgetLayoutConfig
{
    public string ThemeMode { get; set; } = "dark";
    public string PrimaryAccentHex { get; set; } = "#2563EB";
    public int DisplayOrder { get; set; } = 1;
    public int AutoplayIntervalMs { get; set; } = 4500;
    public bool EnableCountdownTimer { get; set; } = true;
}

public sealed class SpinWheelSlice
{
    public required string Label { get; set; }
    public required string CouponCode { get; set; }
    public int DiscountPercent { get; set; }
    public double ProbabilityWeight { get; set; }
}
