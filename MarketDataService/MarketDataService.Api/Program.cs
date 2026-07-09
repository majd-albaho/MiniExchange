using MarketDataService.Application.Interfaces.Services;
using MarketDataService.Infrastructure.Hubs;
using MarketDataService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddGrpc();
builder.Services.AddSingleton<IPriceCache, InMemoryPriceCache>();
builder.Services.AddSingleton<ISubscriptionService, PriceSubscriptionService>();
builder.Services.AddHttpClient<ICandleService, BinanceCandleService>();


builder.Services.AddSignalR();

const string FrontendCorsPolicy = "FrontendCorsPolicy";
builder.Services.AddCors(options => {
    options.AddPolicy(FrontendCorsPolicy, policy => {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();

app.MapHub<MarketDataHub>("/hubs/market-data");

app.MapControllers();
app.MapGrpcService<MarketDataService.Api.Grpc.MarketDataService>();

app.MapGet("/", () => $"Market Data Service is running version {typeof(Program).Assembly.GetName().Version}");

app.Run();
