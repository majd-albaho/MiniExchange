using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.Text.Json.Serialization;
using TradingService.Api.BackgroundServices;
using TradingService.Application;
using TradingService.Infrastructure;
using TradingService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => {
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<TradeExecutedConsumer>();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddGrpc();

const string FrontendCorsPolicy = "FrontendCorsPolicy";
builder.Services.AddCors(options => {
    options.AddPolicy(FrontendCorsPolicy, policy => {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
    await db.Database.MigrateAsync();
}

app.Use(async (context, next) => {
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? context.TraceIdentifier;

    context.Response.Headers["X-Correlation-ID"] = correlationId;

    using (LogContext.PushProperty("CorrelationId", correlationId))
    using (LogContext.PushProperty("TraceId", Activity.Current?.TraceId.ToString()))
    using (LogContext.PushProperty("SpanId", Activity.Current?.SpanId.ToString()))
    using (LogContext.PushProperty("RequestPath", context.Request.Path))
    using (LogContext.PushProperty("RequestMethod", context.Request.Method)) {
        await next();
    }
});

app.UseSerilogRequestLogging(options => {
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) => {
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
    };
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<TradingService.Api.Grpc.TradingService>();

app.MapGet("/", () => $"Trading Service is running version {typeof(Program).Assembly.GetName().Version}");

app.Run();
