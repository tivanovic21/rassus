using Sensor.Clients;
using Sensor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddHttpClient<RestApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.ConnectionClose = false;
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
        MaxConnectionsPerServer = int.MaxValue,
        EnableMultipleHttp2Connections = true,
        ConnectTimeout = TimeSpan.FromSeconds(5)
    };
});

builder.Services.AddSingleton<SensorClientService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<SensorClientService>());

builder.Services.AddSingleton<SensorGrpcService>();

var port = args.Length > 0 ?
    args[0] :
    builder.Configuration["SensorPort"] ?? throw new ArgumentNullException("SensorPort nije pronađen");

// postavlja port iz argumenta u konfig da ga mogu dinamično dohvatiti pri kreiranju novog senzora
builder.Configuration["SensorPort"] = port; 

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(int.Parse(port), listenOptions =>
    {
        // mora http2 jer ako je http1andhttp2 baca grešku pri gRPC pozivu
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });

    options.Limits.MaxConcurrentConnections = 100;
    options.Limits.MaxConcurrentUpgradedConnections = 100;
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
});

var app = builder.Build();

app.MapGrpcService<SensorGrpcService>();

app.MapGet("/", () => "Senzor pokrenut na portu " + port);

app.Run();
