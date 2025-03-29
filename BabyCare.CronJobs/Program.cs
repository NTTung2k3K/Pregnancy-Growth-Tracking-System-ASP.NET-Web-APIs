using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.CronJobs;
using BabyCare.CronJobs.Worker;
using BabyCare.Repositories.Context;
using BabyCare.Repositories.Mapper;
using BabyCare.Repositories.UOW;
using BabyCare.Services.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using VNPAY.NET;

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
builder.Services.AddIdentity<ApplicationUsers, ApplicationRoles>(options =>
{
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    // Identity configuration options
})
          .AddEntityFrameworkStores<DatabaseContext>()
          .AddDefaultTokenProviders();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IVnpay, Vnpay>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
builder.Services.AddScoped<IMembershipPackageService, MembershipPackageService>();

var host = builder.Build();
host.Run();
