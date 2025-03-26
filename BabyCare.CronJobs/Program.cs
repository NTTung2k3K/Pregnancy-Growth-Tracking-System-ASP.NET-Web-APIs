using BabyCare.CronJobs;
using BabyCare.CronJobs.Worker;
using BabyCare.Repositories.Context;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

var builder = Host.CreateApplicationBuilder(args);
// config appsettings by env
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseLazyLoadingProxies().UseMySQL(builder.Configuration.GetConnectionString("BabyCareDb"));
});
builder.Services.AddHostedService<ReminderWorker>();
builder.Services.AddHostedService<FetalGrowthAlertWorker>();
builder.Services.AddHostedService<AppointmentWorker>();

var host = builder.Build();
host.Run();
