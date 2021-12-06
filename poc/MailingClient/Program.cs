using System.Text.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using MimeKit.Text;
using Serilog;

Log.Logger = new LoggerConfiguration()
    // .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .Enrich.WithProperty("ApplicationName", "POC.MailingClient")
    .CreateLogger();

try 
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddUserSecrets<Program>()
        .Build();
    
    var services = new ServiceCollection();
    var serviceProvider = services.BuildServiceProvider();

    Log.Information("Logging works");
    var smtpClientOptions = new SmtpClientOptions();
    configuration.Bind("SmtpClient", smtpClientOptions);
    Log.Information("SMPT Server: {server}, Port: {port}, Ssl: {ssl}", smtpClientOptions.Server, smtpClientOptions.Port, smtpClientOptions.Ssl);

    using var smtpClient = new SmtpClient();
    Log.Information("Connecting to SMTP");
    await smtpClient.ConnectAsync(smtpClientOptions.Server, smtpClientOptions.Port, SecureSocketOptions.StartTls);
    Log.Information("Authenticating using the provided credentials");
    await smtpClient.AuthenticateAsync(smtpClientOptions.Login, smtpClientOptions.Password);
    Log.Information("Ready to send Emails");
    Console.Write("Press any key to send test email...");
    Console.ReadKey();
    Console.WriteLine();
    Log.Information("Sending Email");
    var message = new MimeMessage();
    message.From.Add(MailboxAddress.Parse(smtpClientOptions.Login));
    message.To.Add(MailboxAddress.Parse(smtpClientOptions.Login));
    message.Subject = "Sample Email";
    var content = new 
    {
        FirstName = "John Josua",
        LastName = "Paderon",
        BirthDate = new DateTime(1996, 5, 23),
        Age = 25
    };
    var textPart = new TextPart(TextFormat.Plain) { Text = JsonSerializer.Serialize(content) };
    // var htmlPart = new TextPart(TextFormat.Html) { Text = $"<div>{content.FirstName} - {content.LastName}</div>" };
    message.Body = textPart;
    message.Headers.Add("POC-Mailing__IsFeatureRequest", "true");
    await smtpClient.SendAsync(message);
    Log.Information("Mail sent successfully @ {CurrentDate}", DateTimeOffset.Now);
    Console.Write("Press any key to quit...");
    Log.Information("Disconnecting SMTP");
    await smtpClient.DisconnectAsync(true);
    Log.Information("SMTP disconnected");
    Console.ReadKey();
    Log.Information("Application shutting down...");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An error occured in MailingClient");
}
finally
{
    Log.CloseAndFlush();
}

record SmtpClientOptions
{
    public string Server { get; init; } = "";
    public int Port { get; init; }
    public string Login { get; init; } = "";
    public string Password { get; init; } = "";
    public bool Ssl { get; init; }
}