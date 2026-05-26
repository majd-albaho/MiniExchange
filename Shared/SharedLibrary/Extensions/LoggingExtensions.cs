using Microsoft.Extensions.Hosting;
using Serilog;

namespace SharedLibrary.Extensions
{
    public static class LoggingExtensions
    {
        public static IHostBuilder UseSharedLogger(this IHostBuilder hostBuilder) {
            hostBuilder.UseSerilog((context, configuration) => {
                configuration.Enrich.FromLogContext()
                             .Enrich.WithEnvironmentName()
                             .Enrich.WithProcessId()
                             .Enrich.WithThreadId()
                             .WriteTo.Console();
            });

            return hostBuilder;
        }
    }
}
