using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("StorefrontWidgetCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseCors("StorefrontWidgetCors");
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", gateway = "Ocelot .NET 8", timestamp = DateTime.UtcNow }));

await app.UseOcelot();
app.Run();
