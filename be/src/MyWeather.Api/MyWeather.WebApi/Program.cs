using MyWeather.DAL;
using MyWeather.WebApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
InfraConfiguration.ConfigureServices(builder.Services, builder.Configuration, builder.Environment.ContentRootPath);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // No migrations yet: create the schema on first run.
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<MyWeatherDbContext>().Database.EnsureCreated();
}

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

app.MapCountriesEndpointBuilder();
app.MapWeatherForecastEndpointBuilder();
app.Run();

