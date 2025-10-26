using Server.Repositories;
using Server.Repositories.Interfaces;
using Server.Services;
using Server.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// REPOZITORIJI -> prebaci u Transient ako se doda baza
builder.Services.AddSingleton<ISensorRepository, SensorRepository>();
builder.Services.AddSingleton<IReadingRepository, ReadingRepository>();

// SERVISI
builder.Services.AddScoped<ISensorService, SensorService>();
builder.Services.AddScoped<IReadingService, ReadingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();