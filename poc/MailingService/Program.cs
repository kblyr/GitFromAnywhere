using POC.MailingService;
using Serilog;
using Serilog.Events;

// var configuration = new ConfigurationBuilder()
//     .AddJsonFile("appsettings.json")
//     .Build();

Log.Logger = new LoggerConfiguration()
    // .ReadFrom.Configuration(configuration)
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .Enrich.WithProperty("ApplicationName", "POC.MailingService")
    .CreateLogger();

try
{
     IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureHostConfiguration(builder => {
            builder.AddUserSecrets<Program>();
        })
        .ConfigureServices((context, services) =>
        {
            services.Configure<ImapSettingsConfig>(context.Configuration.GetSection(ImapSettingsConfig.ConfigKey));
            services.AddHostedService<Worker>();
        })
        .ConfigureLogging((context, builder) => {
            builder.AddSerilog(Log.Logger, true);
        })
        .UseSerilog(Log.Logger)
        .Build();

    await host.RunAsync();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker service failed to initialize");
}
finally
{
    Log.CloseAndFlush();
}