using Sensor.Clients;
using Sensor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddHttpClient<RestApiClient>();

builder.Services.AddSingleton<SensorClientService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<SensorClientService>());

builder.Services.AddSingleton<SensorGrpcService>();

var port = args.Length > 0 ?
    args[0] :
    builder.Configuration["SensorPort"] ?? throw new ArgumentNullException("SensorPort nije pronađen");

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(int.Parse(port), listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });
});

var app = builder.Build();

app.MapGrpcService<SensorGrpcService>();

app.MapGet("/", () => "Senzor pokrenut na portu " + port);

app.Run();
