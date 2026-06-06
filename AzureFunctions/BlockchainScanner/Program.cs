using BlockchainScanner.Entites;
using BlockchainScanner.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                Environment.GetEnvironmentVariable(
                    "SqlConnectionString"));
        });

        services.AddSingleton<EthereumScanner>();
    })
    .Build();

host.Run();