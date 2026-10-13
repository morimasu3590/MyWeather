using MyWeather.WebApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
InfraConfiguration.ConfigureServices(builder.Services, builder.Configuration);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", options =>
    {
        options.WithTitle("MyWeather API");
    });
}

app.UseHttpsRedirection();

app.MapWeatherForecastEndpoints();
app.Run();

