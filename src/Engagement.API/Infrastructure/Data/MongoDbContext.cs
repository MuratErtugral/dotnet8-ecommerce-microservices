using Engagement.API.Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Engagement.API.Infrastructure.Data;

public sealed class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "EcommerceEngagementDb";
    public string CampaignsCollection { get; set; } = "WidgetCampaigns";
}

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        _settings = options.Value;
        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }

    public IMongoCollection<WidgetCampaign> Campaigns =>
        _database.GetCollection<WidgetCampaign>(_settings.CampaignsCollection);
}
