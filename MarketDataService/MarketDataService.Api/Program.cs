using MarketDataService.Application.Interfaces.Services;
using MarketDataService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddGrpc();
builder.Services.AddSingleton<IPriceCache, InMemoryPriceCache>();
builder.Services.AddSingleton<ISubscriptionService, PriceSubscriptionService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<MarketDataService.Api.Grpc.MarketDataService>();

app.MapGet("/", () => $"Trading Pair Service is running version {typeof(Program).Assembly.GetName().Version}");

app.Run();
