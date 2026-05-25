var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddApplication();
//builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<WalletService.Api.Grpc.WalletService>();

app.MapGet("/", () => $"Wallet Service is running version {typeof(Program).Assembly.GetName().Version}");

app.Run();
