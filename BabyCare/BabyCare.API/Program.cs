using BabyCare.API;
using BabyCare.WorkerService.Worker;

var builder = WebApplication.CreateBuilder(args);

// config appsettings by env
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();
builder.Services.AddConfig(builder.Configuration);
builder.Services.AddConfigJWT(builder.Configuration);
builder.Services.AddCorsPolicyBackend();

builder.Services.AddHostedService<ReminderWorker>();
builder.Services.AddHostedService<FetalGrowthAlertWorker>();
builder.Services.AddHostedService<AppointmentWorker>();

var app = builder.Build();

// Draft data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        services.SeedData().Wait();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowFrontendLocal");

app.UseCors("AllowFrontendVercel");

app.UseAuthorization();

app.MapControllers();

app.Run();
