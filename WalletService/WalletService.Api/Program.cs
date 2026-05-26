using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Extensions;
using WalletService.Application;
using WalletService.Infrastructure;
using WalletService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSharedLogger();

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<WalletService.Api.BackgroundServices.UserRegisteredConsumer>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddGrpc();

var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<WalletService.Api.Grpc.WalletService>();

app.MapGet("/", () => $"Wallet Service is running version {typeof(Program).Assembly.GetName().Version}");

app.Run();
