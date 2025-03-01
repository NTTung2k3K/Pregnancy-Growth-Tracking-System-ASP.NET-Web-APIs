﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Services.Interface;
using BabyCare.Repositories.Context;
using BabyCare.Services;
using BabyCare.Services.Service;
using BabyCare.Repositories.Mapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using BabyCare.Core.Utils;
using BabyCare.Contract.Services.Implements;
using VNPAY.NET;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.API
{
    public static class DependencyInjection
    {
        public static void AddConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigRoute();
            services.AddDatabase(configuration);
            services.AddIdentity();
            services.AddInfrastructure(configuration);
            services.AddServices();
            services.AddAutoMapperProfiles();
        }

        public static void ConfigRoute(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });
        }

        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseLazyLoadingProxies().UseSqlServer(configuration.GetConnectionString("BabyCareDb"));
            });
        }

        public static void AddIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUsers, ApplicationRoles>(options =>
            {
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                // Identity configuration options
            })
             .AddEntityFrameworkStores<DatabaseContext>()
             .AddDefaultTokenProviders();
        }

        public static void AddServices(this IServiceCollection services)
        {
            services
                .AddScoped<IUserService, UserService>()
                .AddScoped<IRoleService, RoleService>()
                .AddScoped<IVnpay, Vnpay>()
                .AddScoped<IBlogTypeService, BlogTypeService>()
                .AddScoped<IBlogService, BlogService>()
                .AddScoped<IAppointmentService, AppointmentService>()
                .AddScoped<IFetalGrowthRecordService, FetalGrowthRecordService>()
                .AddScoped<IGrowthChartService, GrowthChartService>()
                .AddScoped<IChildService, ChildService>()
                .AddScoped<IFeedbackService, FeedbackService>()
                .AddScoped<IPaymentService, PaymentService>()
                .AddScoped<IVnpay, Vnpay>()
                .AddHttpContextAccessor()
                .AddScoped<IMembershipPackageService, MembershipPackageService>()
                .AddScoped<IAppointmentTemplateService, AppointmentTemplateService>()
                .AddScoped<IFetalGrowthStandardService, FetalGrowthStandardService>();


        }
        public static void AddCorsPolicyBackend(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontendLocal", builder =>
                {
                    builder.WithOrigins("http://localhost:5173") // Các nguồn được phép
                           .AllowAnyMethod()  // Cho phép tất cả các phương thức HTTP (GET, POST, PUT, DELETE,...)
                           .AllowAnyHeader()  // Cho phép tất cả các header
                           .AllowCredentials(); // Cho phép gửi thông tin xác thực (cookies, headers, v.v.)
                });
                options.AddPolicy("AllowFrontendLocalNextJs", builder =>
                {
                    builder.WithOrigins("http://localhost:3000") // Các nguồn được phép
                           .AllowAnyMethod()  // Cho phép tất cả các phương thức HTTP (GET, POST, PUT, DELETE,...)
                           .AllowAnyHeader()  // Cho phép tất cả các header
                           .AllowCredentials(); // Cho phép gửi thông tin xác thực (cookies, headers, v.v.)
                });
                options.AddPolicy("AllowFrontendVercel", builder =>
                {
                    builder.WithOrigins("https://baby-care-theta.vercel.app") // Các nguồn được phép
                           .AllowAnyMethod()  // Cho phép tất cả các phương thức HTTP (GET, POST, PUT, DELETE,...)
                           .AllowAnyHeader()  // Cho phép tất cả các header
                           .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
                           .AllowCredentials(); // Cho phép gửi thông tin xác thực (cookies, headers, v.v.)
                });
            });


        }
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "BabyCare", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });
        }
        public static void AddAutoMapperProfiles(this IServiceCollection services)
        {
            // Register AutoMapper and scan for profiles in the assembly
            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
        }
        public static async Task SeedData(this IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUsers>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRoles>>();
            var dbContext = serviceProvider.GetRequiredService<DatabaseContext>();

            // Seed Roles
            if (!await roleManager.RoleExistsAsync(SystemConstant.Role.ADMIN))
            {
                await roleManager.CreateAsync(new ApplicationRoles() { Name = SystemConstant.Role.ADMIN, ConcurrencyStamp = "1", NormalizedName = SystemConstant.Role.ADMIN.ToUpper() });
            }

            if (!await roleManager.RoleExistsAsync(SystemConstant.Role.DOCTOR))
            {
                await roleManager.CreateAsync(new ApplicationRoles() { Name = SystemConstant.Role.DOCTOR, ConcurrencyStamp = "2", NormalizedName = SystemConstant.Role.DOCTOR.ToUpper() });
            }
            if (!await roleManager.RoleExistsAsync(SystemConstant.Role.USER))
            {
                await roleManager.CreateAsync(new ApplicationRoles() { Name = SystemConstant.Role.USER, ConcurrencyStamp = "3", NormalizedName = SystemConstant.Role.USER.ToUpper() });
            }
            // Seed Accounts
            if (await userManager.FindByNameAsync("Admin1@") == null)
            {
                var adminUser = new ApplicationUsers()
                {
                    UserName = "Admin1@",
                    Email = "admin@example.com",
                    FullName = "This is Admin",
                    Status = (int)SystemConstant.EmployeeStatus.Active,
                    EmailConfirmed = true,
                    Gender = 1,

                    LockoutEnabled = false
                };

                var result = await userManager.CreateAsync(adminUser, "Admin1@");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, SystemConstant.Role.ADMIN);
                }
            }

            if (await userManager.FindByNameAsync("Doctor1@") == null)
            {
                var doctorUser = new ApplicationUsers()
                {
                    UserName = "Doctor1@",
                    FullName = "This is Doctor 1",
                    Email = "doctor@example.com",
                    Status = (int)SystemConstant.EmployeeStatus.Active,
                    Gender = 1,

                    EmailConfirmed = true,
                    LockoutEnabled = false
                };

                var result = await userManager.CreateAsync(doctorUser, "Doctor1@");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(doctorUser, SystemConstant.Role.DOCTOR);
                }
            }

            if (await userManager.FindByNameAsync("Doctor2@") == null)
            {
                var doctorUser = new ApplicationUsers()
                {
                    UserName = "Doctor2@",
                    FullName = "This is Doctor 2",

                    Email = "doctor@example.com",
                    Status = (int)SystemConstant.EmployeeStatus.Active,
                    Gender = 1,

                    EmailConfirmed = true,
                    LockoutEnabled = false
                };

                var result = await userManager.CreateAsync(doctorUser, "Doctor2@");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(doctorUser, SystemConstant.Role.DOCTOR);
                }
            }

            if (await userManager.FindByEmailAsync("User1@gmail.com") == null)
            {
                var normalUser = new ApplicationUsers()
                {
                    UserName = "User1@",
                    Email = "user@gmail.com",
                    FullName = "This is normal user",
                    Status = (int)SystemConstant.EmployeeStatus.Active,
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    Gender = 1
                };

                var result = await userManager.CreateAsync(normalUser, "User1@gmail.com");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, SystemConstant.Role.USER);
                }
            }
            await dbContext.SaveChangesAsync();

            // Seed Appointment Templates
            if (!dbContext.AppointmentTemplates.Any())
            {
                var templates = new List<AppointmentTemplates>
    {
        new AppointmentTemplates
        {
            Name = "First Prenatal Checkup",
            DaysFromBirth = -270,
            Description = "Confirm pregnancy, estimate gestational age, and determine due date.",
            Status = 1,
            Image = "https://cdn-icons-png.flaticon.com/512/3209/3209960.png",
            Fee = 100000 // 100,000 VND
        },
        new AppointmentTemplates
        {
            Name = "First Ultrasound",
            DaysFromBirth = -210,
            Description = "Ultrasound to check fetal morphology for the first time.",
            Status = 1,
            Image = "https://cdn-icons-png.flaticon.com/512/1989/1989553.png",
            Fee = 150000 // 150,000 VND
        },
        new AppointmentTemplates
        {
            Name = "First Blood Test",
            DaysFromBirth = -180,
            Description = "Blood test to screen for genetic disorders and abnormalities.",
            Status = 1,
            Image = "https://cdn-icons-png.flaticon.com/512/1055/1055672.png",
            Fee = 200000 // 200,000 VND
        },
        new AppointmentTemplates
        {
            Name = "Fetal Anomaly Ultrasound",
            DaysFromBirth = -120,
            Description = "Detailed ultrasound to detect congenital abnormalities.",
            Status = 1,
            Image = "https://cdn-icons-png.flaticon.com/512/3209/3209929.png",
            Fee = 250000 // 250,000 VND
        },
        new AppointmentTemplates
        {
            Name = "Gestational Diabetes Test",
            DaysFromBirth = -90,
            Description = "Blood sugar test to check for gestational diabetes.",
            Status = 1,
            Image = "https://cdn-icons-png.flaticon.com/512/2580/2580426.png",
            Fee = 300000 // 300,000 VND
        },
        new AppointmentTemplates
        {
            Name = "First Tetanus Vaccine",
            DaysFromBirth = -60,
            Description = "Tetanus vaccination to protect both mother and baby.",
            Status = 1,
            Image = "https://cdn-icons-png.flaticon.com/512/4210/4210947.png",
            Fee = 50000 // 50,000 VND
        },
        new AppointmentTemplates
        {
            Name = "Growth Monitoring Ultrasound",
            DaysFromBirth = -30,
            Description = "Ultrasound to assess fetal development (weight, amniotic fluid, etc.).",
            Status = 1,
            Image = "https://cdn-icons-png.flaticon.com/512/3209/3209934.png",
            Fee = 200000 // 200,000 VND
        },
        new AppointmentTemplates
        {
            Name = "Final Pregnancy Checkup",
            DaysFromBirth = -7,
            Description = "Health check for mother and baby before delivery.",
            Status = 1,
            Image = "https://cdn-icons-png.flaticon.com/512/3209/3209970.png",
            Fee = 100000 // 100,000 VND
        },
        new AppointmentTemplates
        {
            Name = "Postpartum Checkup",
            DaysFromBirth = 30,
            Description = "Health check for both mother and baby after birth.",
            Status = 1,
            Image = "https://cdn-icons-png.flaticon.com/512/2254/2254821.png",
            Fee = 120000 // 120,000 VND
        }
    };

                dbContext.AppointmentTemplates.AddRange(templates);
                await dbContext.SaveChangesAsync();
            }

            if (!dbContext.MembershipPackages.Any())
            {
                var membershipPackages = new List<MembershipPackage>
{
    new MembershipPackage
    {
        PackageName = "Bronze - Basic Pregnancy Tracking",
Description = "Free package for basic pregnancy tracking.",
        OriginalPrice = 0, // Gói miễn phí
        Discount = 0.00m,
        Price = 0,
        Duration =-1, // 30 ngày
        Status = 1,
        PackageLevel = (int?)PackageLevel.Bronze,
        ShowPriority = 1,
        MaxRecordAdded = 0,
        MaxGrowthChartShares = 0,
        HasGenerateAppointments = false,
        HasStandardDeviationAlerts = false,
        HasViewGrowthChart = false,
        MaxAppointmentCanBooking = 0,

    },
    new MembershipPackage
    {
        PackageName = "Silver - Premium Pregnancy Tracking",
Description = "Premium package for tracking growth charts, scheduling appointments, and receiving notifications.",
        OriginalPrice = 299000, // Giá gốc
        Discount = 0.10m, // 10% giảm giá
        Price = 269100, // Giá sau khi giảm
        Duration = 90, // 3 tháng
        Status = 1,
        PackageLevel = (int?)PackageLevel.Silver,
        ShowPriority = 2,
        MaxRecordAdded = 30,
        MaxGrowthChartShares = 10,
        HasGenerateAppointments = true, // Hỗ trợ tạo lịch hẹn trong 30 ngày
        HasStandardDeviationAlerts = true,
        HasViewGrowthChart = true,
        MaxAppointmentCanBooking = 10,

    },
    new MembershipPackage
    {
        PackageName = "Gold - Ultimate Pregnancy Care",
Description = "Comprehensive package with unlimited features, supporting in-depth tracking.",
        OriginalPrice = 799000, // Giá gốc
        Discount = 0.15m, // 15% giảm giá
        Price = 679150, // Giá sau khi giảm
        Duration = 365, // 1 năm
        Status = 1,
        MaxRecordAdded = -1,
        PackageLevel = (int?)PackageLevel.Gold,
        ShowPriority = 3,
        MaxGrowthChartShares = -1, // Không giới hạn lượt chia sẻ
        HasGenerateAppointments = true, // Hỗ trợ tạo lịch hẹn trong 1 năm
        HasStandardDeviationAlerts = true,
        HasViewGrowthChart = true,
        MaxAppointmentCanBooking = -1,

    }
};

                dbContext.MembershipPackages.AddRange(membershipPackages);
                await dbContext.SaveChangesAsync();
            }

            if (!dbContext.FetalGrowthStandards.Any())
            {
                var fetalGrowthStandards = new List<FetalGrowthStandard>
        {
             // Female
    new FetalGrowthStandard { Week = 1, Gender = 0, GestationalAge = "1 week", MinWeight = 0.01f, MaxWeight = 0.02f, AverageWeight = 0.015f, MinHeight = 0.1f, MaxHeight = 0.15f, AverageHeight = 0.125f, HeadCircumference = 0.2f, AbdominalCircumference = 0.15f, FetalHeartRate = null },
    new FetalGrowthStandard { Week = 2, Gender = 0, GestationalAge = "2 weeks", MinWeight = 0.02f, MaxWeight = 0.03f, AverageWeight = 0.025f, MinHeight = 0.15f, MaxHeight = 0.2f, AverageHeight = 0.175f, HeadCircumference = 0.3f, AbdominalCircumference = 0.25f, FetalHeartRate = null},
    new FetalGrowthStandard { Week = 3, Gender = 0, GestationalAge = "3 weeks", MinWeight = 0.03f, MaxWeight = 0.05f, AverageWeight = 0.04f, MinHeight = 0.2f, MaxHeight = 0.25f, AverageHeight = 0.225f, HeadCircumference = 0.35f, AbdominalCircumference = 0.3f, FetalHeartRate = null },
    new FetalGrowthStandard { Week = 4, Gender = 0, GestationalAge = "4 weeks", MinWeight = 0.05f, MaxWeight = 0.07f, AverageWeight = 0.06f, MinHeight = 0.25f, MaxHeight = 0.3f, AverageHeight = 0.275f, HeadCircumference = 0.4f, AbdominalCircumference = 0.35f, FetalHeartRate = 125 },
    new FetalGrowthStandard { Week = 5, Gender = 0, GestationalAge = "5 weeks", MinWeight = 0.07f, MaxWeight = 0.09f, AverageWeight = 0.08f, MinHeight = 0.3f, MaxHeight = 0.35f, AverageHeight = 0.325f, HeadCircumference = 0.45f, AbdominalCircumference = 0.4f, FetalHeartRate = 130 },
    new FetalGrowthStandard { Week = 6, Gender = 0, GestationalAge = "6 weeks", MinWeight = 0.09f, MaxWeight = 0.12f, AverageWeight = 0.105f, MinHeight = 0.35f, MaxHeight = 0.4f, AverageHeight = 0.375f, HeadCircumference = 0.5f, AbdominalCircumference = 0.45f, FetalHeartRate = 135 },
    new FetalGrowthStandard { Week = 7, Gender = 0, GestationalAge = "7 weeks", MinWeight = 0.12f, MaxWeight = 0.15f, AverageWeight = 0.135f, MinHeight = 0.4f, MaxHeight = 0.45f, AverageHeight = 0.425f, HeadCircumference = 0.55f, AbdominalCircumference = 0.5f, FetalHeartRate = 140 },
    new FetalGrowthStandard { Week = 8, Gender = 0, GestationalAge = "8 weeks", MinWeight = 0.15f, MaxWeight = 0.18f, AverageWeight = 0.165f, MinHeight = 0.45f, MaxHeight = 0.5f, AverageHeight = 0.475f, HeadCircumference = 0.6f, AbdominalCircumference = 0.55f, FetalHeartRate = 145 },
    new FetalGrowthStandard { Week = 9, Gender = 0, GestationalAge = "9 weeks", MinWeight = 0.18f, MaxWeight = 0.22f, AverageWeight = 0.2f, MinHeight = 0.5f, MaxHeight = 0.55f, AverageHeight = 0.525f, HeadCircumference = 0.65f, AbdominalCircumference = 0.6f, FetalHeartRate = 150 },
    new FetalGrowthStandard { Week = 10, Gender = 0, GestationalAge = "10 weeks", MinWeight = 0.22f, MaxWeight = 0.26f, AverageWeight = 0.24f, MinHeight = 0.55f, MaxHeight = 0.6f, AverageHeight = 0.575f, HeadCircumference = 0.7f, AbdominalCircumference = 0.65f, FetalHeartRate = 155 },
    new FetalGrowthStandard { Week = 11, Gender = 0, GestationalAge = "11 weeks", MinWeight = 0.26f, MaxWeight = 0.3f, AverageWeight = 0.28f, MinHeight = 0.6f, MaxHeight = 0.65f, AverageHeight = 0.625f, HeadCircumference = 0.75f, AbdominalCircumference = 0.7f, FetalHeartRate = 160 },
    new FetalGrowthStandard { Week = 12, Gender = 0, GestationalAge = "12 weeks", MinWeight = 0.3f, MaxWeight = 0.35f, AverageWeight = 0.325f, MinHeight = 0.65f, MaxHeight = 0.7f, AverageHeight = 0.675f, HeadCircumference = 0.8f, AbdominalCircumference = 0.75f, FetalHeartRate = 160 },
    new FetalGrowthStandard { Week = 13, Gender = 0, GestationalAge = "13 weeks", MinWeight = 0.35f, MaxWeight = 0.4f, AverageWeight = 0.375f, MinHeight = 0.7f, MaxHeight = 0.75f, AverageHeight = 0.725f, HeadCircumference = 0.85f, AbdominalCircumference = 0.8f, FetalHeartRate = 160 },
    new FetalGrowthStandard { Week = 14, Gender = 0, GestationalAge = "14 weeks", MinWeight = 0.4f, MaxWeight = 0.45f, AverageWeight = 0.425f, MinHeight = 0.75f, MaxHeight = 0.8f, AverageHeight = 0.775f, HeadCircumference = 0.9f, AbdominalCircumference = 0.85f, FetalHeartRate = 155 },
    new FetalGrowthStandard { Week = 15, Gender = 0, GestationalAge = "15 weeks", MinWeight = 0.45f, MaxWeight = 0.5f, AverageWeight = 0.475f, MinHeight = 0.8f, MaxHeight = 0.85f, AverageHeight = 0.825f, HeadCircumference = 0.95f, AbdominalCircumference = 0.9f, FetalHeartRate = 150 },
    new FetalGrowthStandard { Week = 16, Gender = 0, GestationalAge = "16 weeks", MinWeight = 0.5f, MaxWeight = 0.55f, AverageWeight = 0.525f, MinHeight = 0.85f, MaxHeight = 0.9f, AverageHeight = 0.875f, HeadCircumference = 1.0f, AbdominalCircumference = 0.95f, FetalHeartRate = 150 },
    new FetalGrowthStandard { Week = 17, Gender = 0, GestationalAge = "17 weeks", MinWeight = 0.55f, MaxWeight = 0.6f, AverageWeight = 0.575f, MinHeight = 0.9f, MaxHeight = 0.95f, AverageHeight = 0.925f, HeadCircumference = 1.05f, AbdominalCircumference = 1.0f, FetalHeartRate = 145 },
    new FetalGrowthStandard { Week = 18, Gender = 0, GestationalAge = "18 weeks", MinWeight = 0.6f, MaxWeight = 0.65f, AverageWeight = 0.625f, MinHeight = 0.95f, MaxHeight = 1.0f, AverageHeight = 0.975f, HeadCircumference = 1.1f, AbdominalCircumference = 1.05f, FetalHeartRate = 140 },
    new FetalGrowthStandard { Week = 19, Gender = 0, GestationalAge = "19 weeks", MinWeight = 0.65f, MaxWeight = 0.7f, AverageWeight = 0.675f, MinHeight = 1.0f, MaxHeight = 1.05f, AverageHeight = 1.025f, HeadCircumference = 1.15f, AbdominalCircumference = 1.1f, FetalHeartRate = 135 },
    new FetalGrowthStandard { Week = 20, Gender = 0, GestationalAge = "20 weeks", MinWeight = 0.7f, MaxWeight = 0.75f, AverageWeight = 0.725f, MinHeight = 1.05f, MaxHeight = 1.1f, AverageHeight = 1.075f, HeadCircumference = 1.2f, AbdominalCircumference = 1.15f, FetalHeartRate = 130 },
    new FetalGrowthStandard { Week = 21, Gender = 0, GestationalAge = "21 weeks", MinWeight = 0.75f, MaxWeight = 0.8f, AverageWeight = 0.775f, MinHeight = 1.1f, MaxHeight = 1.15f, AverageHeight = 1.125f, HeadCircumference = 1.25f, AbdominalCircumference = 1.2f, FetalHeartRate = 130 },
    new FetalGrowthStandard { Week = 22, Gender = 0, GestationalAge = "22 weeks", MinWeight = 0.8f, MaxWeight = 0.85f, AverageWeight = 0.825f, MinHeight = 1.15f, MaxHeight = 1.2f, AverageHeight = 1.175f, HeadCircumference = 1.3f, AbdominalCircumference = 1.25f, FetalHeartRate = 125 },
    new FetalGrowthStandard { Week = 23, Gender = 0, GestationalAge = "23 weeks", MinWeight = 0.85f, MaxWeight = 0.9f, AverageWeight = 0.875f, MinHeight = 1.2f, MaxHeight = 1.25f, AverageHeight = 1.225f, HeadCircumference = 1.35f, AbdominalCircumference = 1.3f, FetalHeartRate = 120 },
    new FetalGrowthStandard { Week = 24, Gender = 0, GestationalAge = "24 weeks", MinWeight = 0.9f, MaxWeight = 1.0f, AverageWeight = 0.95f, MinHeight = 1.25f, MaxHeight = 1.3f, AverageHeight = 1.275f, HeadCircumference = 1.4f, AbdominalCircumference = 1.35f, FetalHeartRate = 120 },
    new FetalGrowthStandard { Week = 25, Gender = 0, GestationalAge = "25 weeks", MinWeight = 1.0f, MaxWeight = 1.1f, AverageWeight = 1.05f, MinHeight = 1.3f, MaxHeight = 1.35f, AverageHeight = 1.325f, HeadCircumference = 1.45f, AbdominalCircumference = 1.4f, FetalHeartRate = 115 },
    new FetalGrowthStandard { Week = 26, Gender = 0, GestationalAge = "26 weeks", MinWeight = 1.1f, MaxWeight = 1.2f, AverageWeight = 1.15f, MinHeight = 1.35f, MaxHeight = 1.4f, AverageHeight = 1.375f, HeadCircumference = 1.5f, AbdominalCircumference = 1.45f, FetalHeartRate = 115 },
    new FetalGrowthStandard { Week = 27, Gender = 0, GestationalAge = "27 weeks", MinWeight = 1.2f, MaxWeight = 1.3f, AverageWeight = 1.25f, MinHeight = 1.4f, MaxHeight = 1.45f, AverageHeight = 1.425f, HeadCircumference = 1.55f, AbdominalCircumference = 1.5f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 28, Gender = 0, GestationalAge = "28 weeks", MinWeight = 1.3f, MaxWeight = 1.4f, AverageWeight = 1.35f, MinHeight = 1.45f, MaxHeight = 1.5f, AverageHeight = 1.475f, HeadCircumference = 1.6f, AbdominalCircumference = 1.55f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 29, Gender = 0, GestationalAge = "29 weeks", MinWeight = 1.4f, MaxWeight = 1.5f, AverageWeight = 1.45f, MinHeight = 1.5f, MaxHeight = 1.55f, AverageHeight = 1.525f, HeadCircumference = 1.65f, AbdominalCircumference = 1.6f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 30, Gender = 0, GestationalAge = "30 weeks", MinWeight = 1.5f, MaxWeight = 1.6f, AverageWeight = 1.55f, MinHeight = 1.55f, MaxHeight = 1.6f, AverageHeight = 1.575f, HeadCircumference = 1.7f, AbdominalCircumference = 1.65f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 31, Gender = 0, GestationalAge = "31 weeks", MinWeight = 1.6f, MaxWeight = 1.7f, AverageWeight = 1.65f, MinHeight = 1.6f, MaxHeight = 1.65f, AverageHeight = 1.625f, HeadCircumference = 1.75f, AbdominalCircumference = 1.7f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 32, Gender = 0, GestationalAge = "32 weeks", MinWeight = 1.7f, MaxWeight = 1.8f, AverageWeight = 1.75f, MinHeight = 1.65f, MaxHeight = 1.7f, AverageHeight = 1.675f, HeadCircumference = 1.8f, AbdominalCircumference = 1.75f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 33, Gender = 0, GestationalAge = "33 weeks", MinWeight = 1.8f, MaxWeight = 1.9f, AverageWeight = 1.85f, MinHeight = 1.7f, MaxHeight = 1.75f, AverageHeight = 1.725f, HeadCircumference = 1.85f, AbdominalCircumference = 1.8f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 34, Gender = 0, GestationalAge = "34 weeks", MinWeight = 1.9f, MaxWeight = 2.0f, AverageWeight = 1.95f, MinHeight = 1.75f, MaxHeight = 1.8f, AverageHeight = 1.775f, HeadCircumference = 1.9f, AbdominalCircumference = 1.85f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 35, Gender = 0, GestationalAge = "35 weeks", MinWeight = 2.0f, MaxWeight = 2.1f, AverageWeight = 2.05f, MinHeight = 1.8f, MaxHeight = 1.85f, AverageHeight = 1.825f, HeadCircumference = 1.95f, AbdominalCircumference = 1.9f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 36, Gender = 0, GestationalAge = "36 weeks", MinWeight = 2.1f, MaxWeight = 2.2f, AverageWeight = 2.15f, MinHeight = 1.85f, MaxHeight = 1.9f, AverageHeight = 1.875f, HeadCircumference = 2.0f, AbdominalCircumference = 1.95f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 37, Gender = 0, GestationalAge = "37 weeks", MinWeight = 2.2f, MaxWeight = 2.3f, AverageWeight = 2.25f, MinHeight = 1.9f, MaxHeight = 1.95f, AverageHeight = 1.925f, HeadCircumference = 2.05f, AbdominalCircumference = 2.0f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 38, Gender = 0, GestationalAge = "38 weeks", MinWeight = 2.3f, MaxWeight = 2.4f, AverageWeight = 2.35f, MinHeight = 1.95f, MaxHeight = 2.0f, AverageHeight = 1.975f, HeadCircumference = 2.1f, AbdominalCircumference = 2.05f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 39, Gender = 0, GestationalAge = "39 weeks", MinWeight = 2.4f, MaxWeight = 2.5f, AverageWeight = 2.45f, MinHeight = 2.0f, MaxHeight = 2.05f, AverageHeight = 2.025f, HeadCircumference = 2.15f, AbdominalCircumference = 2.1f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 40, Gender = 0, GestationalAge = "40 weeks", MinWeight = 2.5f, MaxWeight = 2.6f, AverageWeight = 2.55f, MinHeight = 2.05f, MaxHeight = 2.1f, AverageHeight = 2.075f, HeadCircumference = 2.2f, AbdominalCircumference = 2.15f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 41, Gender = 0, GestationalAge = "41 weeks", MinWeight = 2.6f, MaxWeight = 2.7f, AverageWeight = 2.65f, MinHeight = 2.1f, MaxHeight = 2.15f, AverageHeight = 2.125f, HeadCircumference = 2.25f, AbdominalCircumference = 2.2f, FetalHeartRate = 110 },

     // Male
    new FetalGrowthStandard { Week = 1, Gender = 1, GestationalAge = "1 week", MinWeight = 0.01f, MaxWeight = 0.02f, AverageWeight = 0.015f, MinHeight = 0.1f, MaxHeight = 0.15f, AverageHeight = 0.125f, HeadCircumference = 0.2f, AbdominalCircumference = 0.15f, FetalHeartRate = null },
    new FetalGrowthStandard { Week = 2, Gender = 1, GestationalAge = "2 weeks", MinWeight = 0.02f, MaxWeight = 0.03f, AverageWeight = 0.025f, MinHeight = 0.15f, MaxHeight = 0.2f, AverageHeight = 0.175f, HeadCircumference = 0.3f, AbdominalCircumference = 0.25f, FetalHeartRate = null },
    new FetalGrowthStandard { Week = 3, Gender = 1, GestationalAge = "3 weeks", MinWeight = 0.03f, MaxWeight = 0.05f, AverageWeight = 0.04f, MinHeight = 0.2f, MaxHeight = 0.25f, AverageHeight = 0.225f, HeadCircumference = 0.35f, AbdominalCircumference = 0.3f, FetalHeartRate = null },
    new FetalGrowthStandard { Week = 4, Gender = 1, GestationalAge = "4 weeks", MinWeight = 0.05f, MaxWeight = 0.07f, AverageWeight = 0.06f, MinHeight = 0.25f, MaxHeight = 0.3f, AverageHeight = 0.275f, HeadCircumference = 0.4f, AbdominalCircumference = 0.35f, FetalHeartRate = 130 },
    new FetalGrowthStandard { Week = 5, Gender = 1, GestationalAge = "5 weeks", MinWeight = 0.07f, MaxWeight = 0.09f, AverageWeight = 0.08f, MinHeight = 0.3f, MaxHeight = 0.35f, AverageHeight = 0.325f, HeadCircumference = 0.45f, AbdominalCircumference = 0.4f, FetalHeartRate = 135 },
    new FetalGrowthStandard { Week = 6, Gender = 1, GestationalAge = "6 weeks", MinWeight = 0.09f, MaxWeight = 0.12f, AverageWeight = 0.105f, MinHeight = 0.35f, MaxHeight = 0.4f, AverageHeight = 0.375f, HeadCircumference = 0.5f, AbdominalCircumference = 0.45f, FetalHeartRate = 140 },
    new FetalGrowthStandard { Week = 7, Gender = 1, GestationalAge = "7 weeks", MinWeight = 0.12f, MaxWeight = 0.15f, AverageWeight = 0.135f, MinHeight = 0.4f, MaxHeight = 0.45f, AverageHeight = 0.425f, HeadCircumference = 0.55f, AbdominalCircumference = 0.5f, FetalHeartRate = 145 },
    new FetalGrowthStandard { Week = 8, Gender = 1, GestationalAge = "8 weeks", MinWeight = 0.15f, MaxWeight = 0.18f, AverageWeight = 0.165f, MinHeight = 0.45f, MaxHeight = 0.5f, AverageHeight = 0.475f, HeadCircumference = 0.6f, AbdominalCircumference = 0.55f, FetalHeartRate = 150 },
    new FetalGrowthStandard { Week = 9, Gender = 1, GestationalAge = "9 weeks", MinWeight = 0.18f, MaxWeight = 0.22f, AverageWeight = 0.2f, MinHeight = 0.5f, MaxHeight = 0.55f, AverageHeight = 0.525f, HeadCircumference = 0.65f, AbdominalCircumference = 0.6f, FetalHeartRate = 155 },
    new FetalGrowthStandard { Week = 10, Gender = 1, GestationalAge = "10 weeks", MinWeight = 0.22f, MaxWeight = 0.26f, AverageWeight = 0.24f, MinHeight = 0.55f, MaxHeight = 0.6f, AverageHeight = 0.575f, HeadCircumference = 0.7f, AbdominalCircumference = 0.65f, FetalHeartRate = 160 },
    new FetalGrowthStandard { Week = 11, Gender = 1, GestationalAge = "11 weeks", MinWeight = 0.26f, MaxWeight = 0.3f, AverageWeight = 0.28f, MinHeight = 0.6f, MaxHeight = 0.65f, AverageHeight = 0.625f, HeadCircumference = 0.75f, AbdominalCircumference = 0.7f, FetalHeartRate = 160 },
    new FetalGrowthStandard { Week = 12, Gender = 1, GestationalAge = "12 weeks", MinWeight = 0.3f, MaxWeight = 0.35f, AverageWeight = 0.325f, MinHeight = 0.65f, MaxHeight = 0.7f, AverageHeight = 0.675f, HeadCircumference = 0.8f, AbdominalCircumference = 0.75f, FetalHeartRate = 160 },
    new FetalGrowthStandard { Week = 13, Gender = 1, GestationalAge = "13 weeks", MinWeight = 0.35f, MaxWeight = 0.4f, AverageWeight = 0.375f, MinHeight = 0.7f, MaxHeight = 0.75f, AverageHeight = 0.725f, HeadCircumference = 0.85f, AbdominalCircumference = 0.8f, FetalHeartRate = 155 },
    new FetalGrowthStandard { Week = 14, Gender = 1, GestationalAge = "14 weeks", MinWeight = 0.4f, MaxWeight = 0.45f, AverageWeight = 0.425f, MinHeight = 0.75f, MaxHeight = 0.8f, AverageHeight = 0.775f, HeadCircumference = 0.9f, AbdominalCircumference = 0.85f, FetalHeartRate = 150 },
    new FetalGrowthStandard { Week = 15, Gender = 1, GestationalAge = "15 weeks", MinWeight = 0.45f, MaxWeight = 0.5f, AverageWeight = 0.475f, MinHeight = 0.8f, MaxHeight = 0.85f, AverageHeight = 0.825f, HeadCircumference = 0.95f, AbdominalCircumference = 0.9f, FetalHeartRate = 145 },
    new FetalGrowthStandard { Week = 16, Gender = 1, GestationalAge = "16 weeks", MinWeight = 0.5f, MaxWeight = 0.55f, AverageWeight = 0.525f, MinHeight = 0.85f, MaxHeight = 0.9f, AverageHeight = 0.875f, HeadCircumference = 1.0f, AbdominalCircumference = 0.95f, FetalHeartRate = 140 },
    new FetalGrowthStandard { Week = 17, Gender = 1, GestationalAge = "17 weeks", MinWeight = 0.55f, MaxWeight = 0.6f, AverageWeight = 0.575f, MinHeight = 0.9f, MaxHeight = 0.95f, AverageHeight = 0.925f, HeadCircumference = 1.05f, AbdominalCircumference = 1.0f, FetalHeartRate = 135 },
    new FetalGrowthStandard { Week = 18, Gender = 1, GestationalAge = "18 weeks", MinWeight = 0.6f, MaxWeight = 0.65f, AverageWeight = 0.625f, MinHeight = 0.95f, MaxHeight = 1.0f, AverageHeight = 0.975f, HeadCircumference = 1.1f, AbdominalCircumference = 1.05f, FetalHeartRate = 130 },
    new FetalGrowthStandard { Week = 19, Gender = 1, GestationalAge = "19 weeks", MinWeight = 0.65f, MaxWeight = 0.7f, AverageWeight = 0.675f, MinHeight = 1.0f, MaxHeight = 1.05f, AverageHeight = 1.025f, HeadCircumference = 1.15f, AbdominalCircumference = 1.1f, FetalHeartRate = 125 },
    new FetalGrowthStandard { Week = 20, Gender = 1, GestationalAge = "20 weeks", MinWeight = 0.7f, MaxWeight = 0.75f, AverageWeight = 0.725f, MinHeight = 1.05f, MaxHeight = 1.1f, AverageHeight = 1.075f, HeadCircumference = 1.2f, AbdominalCircumference = 1.15f, FetalHeartRate = 120 },
    new FetalGrowthStandard { Week = 21, Gender = 1, GestationalAge = "21 weeks", MinWeight = 0.75f, MaxWeight = 0.8f, AverageWeight = 0.775f, MinHeight = 1.1f, MaxHeight = 1.15f, AverageHeight = 1.125f, HeadCircumference = 1.25f, AbdominalCircumference = 1.2f, FetalHeartRate = 120 },
    new FetalGrowthStandard { Week = 22, Gender = 1, GestationalAge = "22 weeks", MinWeight = 0.8f, MaxWeight = 0.85f, AverageWeight = 0.825f, MinHeight = 1.15f, MaxHeight = 1.2f, AverageHeight = 1.175f, HeadCircumference = 1.3f, AbdominalCircumference = 1.25f, FetalHeartRate = 115 },
    new FetalGrowthStandard { Week = 23, Gender = 1, GestationalAge = "23 weeks", MinWeight = 0.85f, MaxWeight = 0.9f, AverageWeight = 0.875f, MinHeight = 1.2f, MaxHeight = 1.25f, AverageHeight = 1.225f, HeadCircumference = 1.35f, AbdominalCircumference = 1.3f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 24, Gender = 1, GestationalAge = "24 weeks", MinWeight = 0.9f, MaxWeight = 1.0f, AverageWeight = 0.95f, MinHeight = 1.25f, MaxHeight = 1.3f, AverageHeight = 1.275f, HeadCircumference = 1.4f, AbdominalCircumference = 1.35f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 25, Gender = 1, GestationalAge = "25 weeks", MinWeight = 1.0f, MaxWeight = 1.1f, AverageWeight = 1.05f, MinHeight = 1.3f, MaxHeight = 1.35f, AverageHeight = 1.325f, HeadCircumference = 1.45f, AbdominalCircumference = 1.4f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 26, Gender = 1, GestationalAge = "26 weeks", MinWeight = 1.1f, MaxWeight = 1.2f, AverageWeight = 1.15f, MinHeight = 1.35f, MaxHeight = 1.4f, AverageHeight = 1.375f, HeadCircumference = 1.5f, AbdominalCircumference = 1.45f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 27, Gender = 1, GestationalAge = "27 weeks", MinWeight = 1.2f, MaxWeight = 1.3f, AverageWeight = 1.25f, MinHeight = 1.4f, MaxHeight = 1.45f, AverageHeight = 1.425f, HeadCircumference = 1.55f, AbdominalCircumference = 1.5f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 28, Gender = 1, GestationalAge = "28 weeks", MinWeight = 1.3f, MaxWeight = 1.4f, AverageWeight = 1.35f, MinHeight = 1.45f, MaxHeight = 1.5f, AverageHeight = 1.475f, HeadCircumference = 1.6f, AbdominalCircumference = 1.55f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 29, Gender = 1, GestationalAge = "29 weeks", MinWeight = 1.4f, MaxWeight = 1.5f, AverageWeight = 1.45f, MinHeight = 1.5f, MaxHeight = 1.55f, AverageHeight = 1.525f, HeadCircumference = 1.65f, AbdominalCircumference = 1.6f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 30, Gender = 1, GestationalAge = "30 weeks", MinWeight = 1.5f, MaxWeight = 1.6f, AverageWeight = 1.55f, MinHeight = 1.55f, MaxHeight = 1.6f, AverageHeight = 1.575f, HeadCircumference = 1.7f, AbdominalCircumference = 1.65f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 31, Gender = 1, GestationalAge = "31 weeks", MinWeight = 1.6f, MaxWeight = 1.7f, AverageWeight = 1.65f, MinHeight = 1.6f, MaxHeight = 1.65f, AverageHeight = 1.625f, HeadCircumference = 1.75f, AbdominalCircumference = 1.7f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 32, Gender = 1, GestationalAge = "32 weeks", MinWeight = 1.7f, MaxWeight = 1.8f, AverageWeight = 1.75f, MinHeight = 1.65f, MaxHeight = 1.7f, AverageHeight = 1.675f, HeadCircumference = 1.8f, AbdominalCircumference = 1.75f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 33, Gender = 1, GestationalAge = "33 weeks", MinWeight = 1.8f, MaxWeight = 1.9f, AverageWeight = 1.85f, MinHeight = 1.7f, MaxHeight = 1.75f, AverageHeight = 1.725f, HeadCircumference = 1.85f, AbdominalCircumference = 1.8f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 34, Gender = 1, GestationalAge = "34 weeks", MinWeight = 1.9f, MaxWeight = 2.0f, AverageWeight = 1.95f, MinHeight = 1.75f, MaxHeight = 1.8f, AverageHeight = 1.775f, HeadCircumference = 1.9f, AbdominalCircumference = 1.85f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 35, Gender = 1, GestationalAge = "35 weeks", MinWeight = 2.0f, MaxWeight = 2.1f, AverageWeight = 2.05f, MinHeight = 1.8f, MaxHeight = 1.85f, AverageHeight = 1.825f, HeadCircumference = 1.95f, AbdominalCircumference = 1.9f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 36, Gender = 1, GestationalAge = "36 weeks", MinWeight = 2.1f, MaxWeight = 2.2f, AverageWeight = 2.15f, MinHeight = 1.85f, MaxHeight = 1.9f, AverageHeight = 1.875f, HeadCircumference = 2.0f, AbdominalCircumference = 1.95f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 37, Gender = 1, GestationalAge = "37 weeks", MinWeight = 2.2f, MaxWeight = 2.3f, AverageWeight = 2.25f, MinHeight = 1.9f, MaxHeight = 1.95f, AverageHeight = 1.925f, HeadCircumference = 2.05f, AbdominalCircumference = 2.0f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 38, Gender = 1, GestationalAge = "38 weeks", MinWeight = 2.3f, MaxWeight = 2.4f, AverageWeight = 2.35f, MinHeight = 1.95f, MaxHeight = 2.0f, AverageHeight = 1.975f, HeadCircumference = 2.1f, AbdominalCircumference = 2.05f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 39, Gender = 1, GestationalAge = "39 weeks", MinWeight = 2.4f, MaxWeight = 2.5f, AverageWeight = 2.45f, MinHeight = 2.0f, MaxHeight = 2.05f, AverageHeight = 2.025f, HeadCircumference = 2.15f, AbdominalCircumference = 2.1f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 40, Gender = 1, GestationalAge = "40 weeks", MinWeight = 2.5f, MaxWeight = 2.6f, AverageWeight = 2.55f, MinHeight = 2.05f, MaxHeight = 2.1f, AverageHeight = 2.075f, HeadCircumference = 2.2f, AbdominalCircumference = 2.15f, FetalHeartRate = 110 },
    new FetalGrowthStandard { Week = 41, Gender = 1, GestationalAge = "41 weeks", MinWeight = 2.6f, MaxWeight = 2.7f, AverageWeight = 2.65f, MinHeight = 2.1f, MaxHeight = 2.15f, AverageHeight = 2.125f, HeadCircumference = 2.25f, AbdominalCircumference = 2.2f, FetalHeartRate = 110 }


                };

                dbContext.FetalGrowthStandards.AddRange(fetalGrowthStandards);
                await dbContext.SaveChangesAsync();
            }
        }

        public static void AddConfigJWT(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = configuration["JWT:ValidAudience"],
                    ValidIssuer = configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"])),
                    ClockSkew = TimeSpan.Zero,
                    ValidateLifetime = true,
                    RoleClaimType = ClaimTypes.Role
                };
            });
        }
    }
}
