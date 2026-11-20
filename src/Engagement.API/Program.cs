using Engagement.API.Infrastructure.Data;
using Engagement.API.Infrastructure.Messaging;
using Engagement.API.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddSingleton<IKafkaEventProducer, KafkaEventProducer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "E-Commerce Storefront Engagement Microservice (.NET 8)",
        Version = "v1",
        Description = "High-throughput microservice powering storefront DOM widgets (Flash Products, Spin-the-Wheel, Smart Cart Recommendations) with MongoDB & Apache Kafka."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new
{
    service = "Engagement.API",
    runtime = ".NET 8.0",
    status = "Healthy",
    timestampUtc = DateTime.UtcNow
}));

app.MapControllers();
app.Run();
