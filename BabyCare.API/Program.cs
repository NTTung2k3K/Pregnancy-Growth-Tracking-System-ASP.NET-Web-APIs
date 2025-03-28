using BabyCare.API;
using BabyCare.WorkerService.Worker;
using StackExchange.Redis;

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
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(new ConfigurationOptions
{
    EndPoints = { builder.Configuration["Redis:EndPoints"] }, // Chỉ host:port, không có redis://
    Password = builder.Configuration["Redis:Password"], // Thêm mật khẩu
    ConnectTimeout = 10000, // Tăng thời gian chờ kết nối lên 10 giây
    ConnectRetry = 3, // Số lần thử kết nối lại
    AbortOnConnectFail = false, // Không ngắt kết nối nếu không thành công
    SyncTimeout = 10000 // Tăng thời gian đồng bộ
}));
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

if (builder.Environment.IsProduction() && builder.Configuration.GetValue<int?>("PORT") is not null)
{
    builder.WebHost.UseUrls($"http://*:{builder.Configuration.GetValue<int>("PORT")}");
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");


app.UseAuthorization();

app.MapControllers();

app.Run();
