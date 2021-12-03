using POC.MailingService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(builder => {
        builder.AddUserSecrets<Program>();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<ImapSettingsConfig>(context.Configuration.GetSection(ImapSettingsConfig.ConfigKey));
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
