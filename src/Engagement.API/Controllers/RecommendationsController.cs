using Engagement.API.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Engagement.API.Controllers;

[ApiController]
[Route("api/v1/recommendations")]
public sealed class RecommendationsController : ControllerBase
{
    [HttpPost("cart-upsell")]
    public IActionResult EvaluateCartRecommendations([FromBody] CartEvaluationRequest request)
    {
        // High-speed rule evaluation for storefront cart drawer upsell widget
        var freeShippingThreshold = 1500m;
        var remainingForFreeShipping = Math.Max(0, freeShippingThreshold - request.CartSubtotal);

        var recommendations = new[]
        {
            new
            {
                sku = "FLASH-ACC-01",
                title = "Express Care Kit & Protector",
                price = 199.90m,
                badge = remainingForFreeShipping > 0 ? "Completes Free Shipping" : "Flash 30% Off",
                confidenceScore = 0.94
            },
            new
            {
                sku = "BUNDLE-UP-02",
                title = "Premium Gift Packaging Add-on",
                price = 89.50m,
                badge = "Most Added to Cart",
                confidenceScore = 0.88
            }
        };

        return Ok(new
        {
            tenantId = request.TenantId,
            platform = request.Platform,
            cartSubtotal = request.CartSubtotal,
            freeShippingUnlocked = remainingForFreeShipping == 0,
            remainingForFreeShipping,
            suggestedItems = recommendations
        });
    }

    [HttpPost("spin-wheel/resolve")]
    public IActionResult ResolveSpinWheel([FromBody] List<SpinWheelSlice> slices)
    {
        if (slices is null || slices.Count == 0)
            return BadRequest("Wheel slices configuration is required.");

        var totalWeight = slices.Sum(s => Math.Max(0.01, s.ProbabilityWeight));
        var roll = Random.Shared.NextDouble() * totalWeight;
        double cumulative = 0;

        foreach (var slice in slices)
        {
            cumulative += Math.Max(0.01, slice.ProbabilityWeight);
            if (roll <= cumulative)
            {
                return Ok(new
                {
                    awardedLabel = slice.Label,
                    couponCode = slice.CouponCode,
                    discountPercent = slice.DiscountPercent,
                    issuedAtUtc = DateTime.UtcNow
                });
            }
        }

        return Ok(slices[0]);
    }
}

public sealed record CartEvaluationRequest(
    string TenantId,
    string Platform,
    decimal CartSubtotal,
    List<string> CurrentCartSkus
);
