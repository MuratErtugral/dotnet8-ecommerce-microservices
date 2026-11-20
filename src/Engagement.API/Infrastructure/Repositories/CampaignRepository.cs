using Engagement.API.Domain.Entities;
using Engagement.API.Infrastructure.Data;
using MongoDB.Driver;

namespace Engagement.API.Infrastructure.Repositories;

public interface ICampaignRepository
{
    Task<IReadOnlyList<WidgetCampaign>> GetActiveByTenantAsync(string tenantId, CancellationToken ct = default);
    Task<WidgetCampaign?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<WidgetCampaign> CreateAsync(WidgetCampaign campaign, CancellationToken ct = default);
    Task<bool> UpdateLayoutAsync(string id, WidgetLayoutConfig layoutConfig, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}

public sealed class CampaignRepository(MongoDbContext context) : ICampaignRepository
{
    public async Task<IReadOnlyList<WidgetCampaign>> GetActiveByTenantAsync(string tenantId, CancellationToken ct = default)
    {
        var filter = Builders<WidgetCampaign>.Filter.And(
            Builders<WidgetCampaign>.Filter.Eq(x => x.TenantId, tenantId),
            Builders<WidgetCampaign>.Filter.Eq(x => x.IsActive, true)
        );

        return await context.Campaigns
            .Find(filter)
            .SortBy(x => x.LayoutConfig.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<WidgetCampaign?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await context.Campaigns
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<WidgetCampaign> CreateAsync(WidgetCampaign campaign, CancellationToken ct = default)
    {
        campaign.UpdatedAtUtc = DateTime.UtcNow;
        await context.Campaigns.InsertOneAsync(campaign, cancellationToken: ct);
        return campaign;
    }

    public async Task<bool> UpdateLayoutAsync(string id, WidgetLayoutConfig layoutConfig, CancellationToken ct = default)
    {
        var update = Builders<WidgetCampaign>.Update
            .Set(x => x.LayoutConfig, layoutConfig)
            .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);

        var result = await context.Campaigns.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var result = await context.Campaigns.DeleteOneAsync(x => x.Id == id, ct);
        return result.DeletedCount > 0;
    }
}
