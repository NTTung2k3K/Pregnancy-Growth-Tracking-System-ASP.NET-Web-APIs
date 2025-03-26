using BabyCare.CronJobs;
using BabyCare.CronJobs.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<ReminderWorker>();
builder.Services.AddHostedService<FetalGrowthAlertWorker>();
builder.Services.AddHostedService<AppointmentWorker>();

var host = builder.Build();
host.Run();
