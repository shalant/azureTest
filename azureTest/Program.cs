using azureTest.Config;
using azureTest.Services.Interfaces;
using azureTest.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var openWeatherConfig = builder.Configuration.GetSection("OpenWeather");
var usdaConfig = builder.Configuration.GetSection("Usda");
builder.Services.Configure<OpenWeather>(openWeatherConfig);
builder.Services.Configure<Usda>(usdaConfig);
builder.Services.AddHttpClient();

builder.Services.AddScoped<IUsdaInfoService, UsdaInfoService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
}

app.UseCors(options =>
{
    options.AllowAnyHeader();
    options.AllowAnyOrigin();
    options.AllowAnyMethod();
});

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
