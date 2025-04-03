using QisWatchdogService;
using Serilog;

var configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json")
               .Build();

Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

try
{
    Log.Information("WatchdogService starting at {Time}", DateTime.Now);
    IHost host = Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;
                    

                    services.Configure<List<ServiceTracking>>(configuration.GetSection("ServiceTracking"));
                    services.AddHostedService<WatchdogWorkerService>();
                    services.Configure<GlobalSettings>(configuration.GetSection("GlobalSettings"));
                })
                .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "WatchdogService Can't start! at {Time}", DateTime.Now);
}
finally
{
    Log.CloseAndFlush();
}


