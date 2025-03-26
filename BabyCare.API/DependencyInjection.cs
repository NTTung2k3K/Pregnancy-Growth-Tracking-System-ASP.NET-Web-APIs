using Microsoft.AspNetCore.Identity;
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
                options.UseLazyLoadingProxies().UseMySQL(configuration.GetConnectionString("BabyCareDb"));
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
                .AddScoped<IFetalGrowthStandardService, FetalGrowthStandardService>()
                .AddScoped<IAIChildService, AIChildService>()
                .AddScoped<IRealTimeService, RealTimeService>()
                .AddScoped<IAIWebsiteService, AIWebsiteService>();


        }
        public static void AddCorsPolicyBackend(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", builder =>
                {
                    builder
                        .WithOrigins(
                            "http://localhost:5173",
                            "http://localhost:3000",
                            "https://baby-care-theta.vercel.app"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
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

            if (await userManager.FindByEmailAsync("user@gmail.com") == null)
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
                    // Female (Giới tính nữ - Gender = 0)
new FetalGrowthStandard { Week = 1, Gender = 0, GestationalAge = "1 week", MinWeight = 0.01f, MaxWeight = 0.02f, AverageWeight = 0.015f, MinHeight = 0.1f, MaxHeight = 0.15f, AverageHeight = 0.125f, HeadCircumference = 0.2f, AbdominalCircumference = 0.15f, FetalHeartRate = null },
new FetalGrowthStandard { Week = 2, Gender = 0, GestationalAge = "2 weeks", MinWeight = 0.02f, MaxWeight = 0.03f, AverageWeight = 0.025f, MinHeight = 0.15f, MaxHeight = 0.2f, AverageHeight = 0.175f, HeadCircumference = 0.3f, AbdominalCircumference = 0.25f, FetalHeartRate = null },
new FetalGrowthStandard { Week = 3, Gender = 0, GestationalAge = "3 weeks", MinWeight = 0.03f, MaxWeight = 0.05f, AverageWeight = 0.04f, MinHeight = 0.2f, MaxHeight = 0.25f, AverageHeight = 0.225f, HeadCircumference = 0.35f, AbdominalCircumference = 0.3f, FetalHeartRate = null },
new FetalGrowthStandard { Week = 4, Gender = 0, GestationalAge = "4 weeks", MinWeight = 0.05f, MaxWeight = 0.07f, AverageWeight = 0.06f, MinHeight = 0.25f, MaxHeight = 0.3f, AverageHeight = 0.275f, HeadCircumference = 0.4f, AbdominalCircumference = 0.35f, FetalHeartRate = 115 },
new FetalGrowthStandard { Week = 5, Gender = 0, GestationalAge = "5 weeks", MinWeight = 0.07f, MaxWeight = 0.09f, AverageWeight = 0.08f, MinHeight = 0.3f, MaxHeight = 0.35f, AverageHeight = 0.325f, HeadCircumference = 0.45f, AbdominalCircumference = 0.4f, FetalHeartRate = 120 },
new FetalGrowthStandard { Week = 6, Gender = 0, GestationalAge = "6 weeks", MinWeight = 0.09f, MaxWeight = 0.12f, AverageWeight = 0.105f, MinHeight = 0.35f, MaxHeight = 0.4f, AverageHeight = 0.375f, HeadCircumference = 0.5f, AbdominalCircumference = 0.45f, FetalHeartRate = 125 },
new FetalGrowthStandard { Week = 7, Gender = 0, GestationalAge = "7 weeks", MinWeight = 0.12f, MaxWeight = 0.15f, AverageWeight = 0.135f, MinHeight = 0.4f, MaxHeight = 0.45f, AverageHeight = 0.425f, HeadCircumference = 0.55f, AbdominalCircumference = 0.5f, FetalHeartRate = 130 },
new FetalGrowthStandard { Week = 8, Gender = 0, GestationalAge = "8 weeks", MinWeight = 0.8f, MaxWeight = 1.2f, AverageWeight = 1.0f, MinHeight = 1.4f, MaxHeight = 1.8f, AverageHeight = 1.6f, HeadCircumference = 0.6f, AbdominalCircumference = 0.55f, FetalHeartRate = 130 },
new FetalGrowthStandard { Week = 9, Gender = 0, GestationalAge = "9 weeks", MinWeight = 1.6f, MaxWeight = 2.4f, AverageWeight = 2.0f, MinHeight = 2.1f, MaxHeight = 2.5f, AverageHeight = 2.3f, HeadCircumference = 0.65f, AbdominalCircumference = 0.6f, FetalHeartRate = 135 },
new FetalGrowthStandard { Week = 10, Gender = 0, GestationalAge = "10 weeks", MinWeight = 3.2f, MaxWeight = 4.8f, AverageWeight = 4.0f, MinHeight = 2.9f, MaxHeight = 3.3f, AverageHeight = 3.1f, HeadCircumference = 0.7f, AbdominalCircumference = 0.65f, FetalHeartRate = 140 },
new FetalGrowthStandard { Week = 11, Gender = 0, GestationalAge = "11 weeks", MinWeight = 5.6f, MaxWeight = 8.4f, AverageWeight = 7.0f, MinHeight = 3.9f, MaxHeight = 4.3f, AverageHeight = 4.1f, HeadCircumference = 0.75f, AbdominalCircumference = 0.7f, FetalHeartRate = 140 },
new FetalGrowthStandard { Week = 12, Gender = 0, GestationalAge = "12 weeks", MinWeight = 11.2f, MaxWeight = 16.8f, AverageWeight = 14.0f, MinHeight = 5.2f, MaxHeight = 5.6f, AverageHeight = 5.4f, HeadCircumference = 0.8f, AbdominalCircumference = 0.75f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 13, Gender = 0, GestationalAge = "13 weeks", MinWeight = 18.4f, MaxWeight = 27.6f, AverageWeight = 23.0f, MinHeight = 7.2f, MaxHeight = 7.6f, AverageHeight = 7.4f, HeadCircumference = 0.85f, AbdominalCircumference = 0.8f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 14, Gender = 0, GestationalAge = "14 weeks", MinWeight = 34.4f, MaxWeight = 51.6f, AverageWeight = 43.0f, MinHeight = 8.5f, MaxHeight = 8.9f, AverageHeight = 8.7f, HeadCircumference = 0.9f, AbdominalCircumference = 0.85f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 15, Gender = 0, GestationalAge = "15 weeks", MinWeight = 56.0f, MaxWeight = 84.0f, AverageWeight = 70.0f, MinHeight = 9.9f, MaxHeight = 10.3f, AverageHeight = 10.1f, HeadCircumference = 0.95f, AbdominalCircumference = 0.9f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 16, Gender = 0, GestationalAge = "16 weeks", MinWeight = 80.0f, MaxWeight = 120.0f, AverageWeight = 100.0f, MinHeight = 11.4f, MaxHeight = 11.8f, AverageHeight = 11.6f, HeadCircumference = 1.0f, AbdominalCircumference = 0.95f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 17, Gender = 0, GestationalAge = "17 weeks", MinWeight = 112.0f, MaxWeight = 168.0f, AverageWeight = 140.0f, MinHeight = 12.8f, MaxHeight = 13.2f, AverageHeight = 13.0f, HeadCircumference = 1.05f, AbdominalCircumference = 1.0f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 18, Gender = 0, GestationalAge = "18 weeks", MinWeight = 152.0f, MaxWeight = 228.0f, AverageWeight = 190.0f, MinHeight = 14.0f, MaxHeight = 14.4f, AverageHeight = 14.2f, HeadCircumference = 1.1f, AbdominalCircumference = 1.05f, FetalHeartRate = 140 },
new FetalGrowthStandard { Week = 19, Gender = 0, GestationalAge = "19 weeks", MinWeight = 192.0f, MaxWeight = 288.0f, AverageWeight = 240.0f, MinHeight = 15.1f, MaxHeight = 15.5f, AverageHeight = 15.3f, HeadCircumference = 1.15f, AbdominalCircumference = 1.1f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 20, Gender = 0, GestationalAge = "20 weeks", MinWeight = 240.0f, MaxWeight = 360.0f, AverageWeight = 300.0f, MinHeight = 16.2f, MaxHeight = 16.6f, AverageHeight = 16.4f, HeadCircumference = 1.2f, AbdominalCircumference = 1.15f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 21, Gender = 0, GestationalAge = "21 weeks", MinWeight = 288.0f, MaxWeight = 432.0f, AverageWeight = 360.0f, MinHeight = 25.4f, MaxHeight = 25.8f, AverageHeight = 25.6f, HeadCircumference = 1.25f, AbdominalCircumference = 1.2f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 22, Gender = 0, GestationalAge = "22 weeks", MinWeight = 344.0f, MaxWeight = 516.0f, AverageWeight = 430.0f, MinHeight = 27.6f, MaxHeight = 28.0f, AverageHeight = 27.8f, HeadCircumference = 1.3f, AbdominalCircumference = 1.25f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 23, Gender = 0, GestationalAge = "23 weeks", MinWeight = 400.8f, MaxWeight = 601.2f, AverageWeight = 501.0f, MinHeight = 28.7f, MaxHeight = 29.1f, AverageHeight = 28.9f, HeadCircumference = 1.35f, AbdominalCircumference = 1.3f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 24, Gender = 0, GestationalAge = "24 weeks", MinWeight = 480.0f, MaxWeight = 720.0f, AverageWeight = 600.0f, MinHeight = 29.8f, MaxHeight = 30.2f, AverageHeight = 30.0f, HeadCircumference = 1.4f, AbdominalCircumference = 1.35f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 25, Gender = 0, GestationalAge = "25 weeks", MinWeight = 528.0f, MaxWeight = 792.0f, AverageWeight = 660.0f, MinHeight = 34.4f, MaxHeight = 34.8f, AverageHeight = 34.6f, HeadCircumference = 1.45f, AbdominalCircumference = 1.4f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 26, Gender = 0, GestationalAge = "26 weeks", MinWeight = 608.0f, MaxWeight = 912.0f, AverageWeight = 760.0f, MinHeight = 35.4f, MaxHeight = 35.8f, AverageHeight = 35.6f, HeadCircumference = 1.5f, AbdominalCircumference = 1.45f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 27, Gender = 0, GestationalAge = "27 weeks", MinWeight = 700.0f, MaxWeight = 1050.0f, AverageWeight = 875.0f, MinHeight = 36.4f, MaxHeight = 36.8f, AverageHeight = 36.6f, HeadCircumference = 1.55f, AbdominalCircumference = 1.5f, FetalHeartRate = 150 },
new FetalGrowthStandard { Week = 28, Gender = 0, GestationalAge = "28 weeks", MinWeight = 804.0f, MaxWeight = 1206.0f, AverageWeight = 1005.0f, MinHeight = 37.4f, MaxHeight = 37.8f, AverageHeight = 37.6f, HeadCircumference = 1.6f, AbdominalCircumference = 1.55f, FetalHeartRate = 150 },
new FetalGrowthStandard { Week = 29, Gender = 0, GestationalAge = "29 weeks", MinWeight = 922.4f, MaxWeight = 1383.6f, AverageWeight = 1153.0f, MinHeight = 38.4f, MaxHeight = 38.8f, AverageHeight = 38.6f, HeadCircumference = 1.65f, AbdominalCircumference = 1.6f, FetalHeartRate = 150 },
new FetalGrowthStandard { Week = 30, Gender = 0, GestationalAge = "30 weeks", MinWeight = 1055.2f, MaxWeight = 1582.8f, AverageWeight = 1319.0f, MinHeight = 39.7f, MaxHeight = 40.1f, AverageHeight = 39.9f, HeadCircumference = 1.7f, AbdominalCircumference = 1.65f, FetalHeartRate = 150 },
new FetalGrowthStandard { Week = 31, Gender = 0, GestationalAge = "31 weeks", MinWeight = 1201.6f, MaxWeight = 1802.4f, AverageWeight = 1502.0f, MinHeight = 40.9f, MaxHeight = 41.3f, AverageHeight = 41.1f, HeadCircumference = 1.75f, AbdominalCircumference = 1.7f, FetalHeartRate = 150 },
new FetalGrowthStandard { Week = 32, Gender = 0, GestationalAge = "32 weeks", MinWeight = 1361.6f, MaxWeight = 2042.4f, AverageWeight = 1702.0f, MinHeight = 42.2f, MaxHeight = 42.6f, AverageHeight = 42.4f, HeadCircumference = 1.8f, AbdominalCircumference = 1.75f, FetalHeartRate = 150 },
new FetalGrowthStandard { Week = 33, Gender = 0, GestationalAge = "33 weeks", MinWeight = 1534.4f, MaxWeight = 2301.6f, AverageWeight = 1918.0f, MinHeight = 43.5f, MaxHeight = 43.9f, AverageHeight = 43.7f, HeadCircumference = 1.85f, AbdominalCircumference = 1.8f, FetalHeartRate = 155 },
new FetalGrowthStandard { Week = 34, Gender = 0, GestationalAge = "34 weeks", MinWeight = 1716.8f, MaxWeight = 2575.2f, AverageWeight = 2146.0f, MinHeight = 44.8f, MaxHeight = 45.2f, AverageHeight = 45.0f, HeadCircumference = 1.9f, AbdominalCircumference = 1.85f, FetalHeartRate = 155 },
new FetalGrowthStandard { Week = 35, Gender = 0, GestationalAge = "35 weeks", MinWeight = 1906.4f, MaxWeight = 2859.6f, AverageWeight = 2383.0f, MinHeight = 46.0f, MaxHeight = 46.4f, AverageHeight = 46.2f, HeadCircumference = 1.95f, AbdominalCircumference = 1.9f, FetalHeartRate = 155 },
new FetalGrowthStandard { Week = 36, Gender = 0, GestationalAge = "36 weeks", MinWeight = 2097.6f, MaxWeight = 3146.4f, AverageWeight = 2622.0f, MinHeight = 47.2f, MaxHeight = 47.6f, AverageHeight = 47.4f, HeadCircumference = 2.0f, AbdominalCircumference = 1.95f, FetalHeartRate = 155 },
new FetalGrowthStandard { Week = 37, Gender = 0, GestationalAge = "37 weeks", MinWeight = 2287.2f, MaxWeight = 3430.8f, AverageWeight = 2859.0f, MinHeight = 48.4f, MaxHeight = 48.8f, AverageHeight = 48.6f, HeadCircumference = 2.05f, AbdominalCircumference = 2.0f, FetalHeartRate = 160 },
new FetalGrowthStandard { Week = 38, Gender = 0, GestationalAge = "38 weeks", MinWeight = 2466.4f, MaxWeight = 3699.6f, AverageWeight = 3083.0f, MinHeight = 49.6f, MaxHeight = 50.0f, AverageHeight = 49.8f, HeadCircumference = 2.1f, AbdominalCircumference = 2.05f, FetalHeartRate = 160 },
new FetalGrowthStandard { Week = 39, Gender = 0, GestationalAge = "39 weeks", MinWeight = 2630.4f, MaxWeight = 3945.6f, AverageWeight = 3288.0f, MinHeight = 50.5f, MaxHeight = 50.9f, AverageHeight = 50.7f, HeadCircumference = 2.15f, AbdominalCircumference = 2.1f, FetalHeartRate = 160 },
new FetalGrowthStandard { Week = 40, Gender = 0, GestationalAge = "40 weeks", MinWeight = 2769.6f, MaxWeight = 4154.4f, AverageWeight = 3462.0f, MinHeight = 51.0f, MaxHeight = 51.4f, AverageHeight = 51.2f, HeadCircumference = 2.2f, AbdominalCircumference = 2.15f, FetalHeartRate = 160 },
new FetalGrowthStandard { Week = 41, Gender = 1, GestationalAge = "41 weeks", MinWeight = 2799.6f, MaxWeight = 4254.4f, AverageWeight = 3492.0f, MinHeight = 51.5f, MaxHeight = 51.5f, AverageHeight = 51.5f, HeadCircumference = 2.25f, AbdominalCircumference = 2.2f, FetalHeartRate = 160 },

// Male (Giới tính nam - Gender = 1)
new FetalGrowthStandard { Week = 1, Gender = 1, GestationalAge = "1 week", MinWeight = 0.01f, MaxWeight = 0.02f, AverageWeight = 0.015f, MinHeight = 0.1f, MaxHeight = 0.15f, AverageHeight = 0.125f, HeadCircumference = 0.2f, AbdominalCircumference = 0.15f, FetalHeartRate = null },
new FetalGrowthStandard { Week = 2, Gender = 1, GestationalAge = "2 weeks", MinWeight = 0.02f, MaxWeight = 0.03f, AverageWeight = 0.025f, MinHeight = 0.15f, MaxHeight = 0.2f, AverageHeight = 0.175f, HeadCircumference = 0.3f, AbdominalCircumference = 0.25f, FetalHeartRate = null },
new FetalGrowthStandard { Week = 3, Gender = 1, GestationalAge = "3 weeks", MinWeight = 0.03f, MaxWeight = 0.05f, AverageWeight = 0.04f, MinHeight = 0.2f, MaxHeight = 0.25f, AverageHeight = 0.225f, HeadCircumference = 0.35f, AbdominalCircumference = 0.3f, FetalHeartRate = null },
new FetalGrowthStandard { Week = 4, Gender = 1, GestationalAge = "4 weeks", MinWeight = 0.05f, MaxWeight = 0.07f, AverageWeight = 0.06f, MinHeight = 0.25f, MaxHeight = 0.3f, AverageHeight = 0.275f, HeadCircumference = 0.4f, AbdominalCircumference = 0.35f, FetalHeartRate = 115 },
new FetalGrowthStandard { Week = 5, Gender = 1, GestationalAge = "5 weeks", MinWeight = 0.07f, MaxWeight = 0.09f, AverageWeight = 0.08f, MinHeight = 0.3f, MaxHeight = 0.35f, AverageHeight = 0.325f, HeadCircumference = 0.45f, AbdominalCircumference = 0.4f, FetalHeartRate = 120 },
new FetalGrowthStandard { Week = 6, Gender = 1, GestationalAge = "6 weeks", MinWeight = 0.09f, MaxWeight = 0.12f, AverageWeight = 0.105f, MinHeight = 0.35f, MaxHeight = 0.4f, AverageHeight = 0.375f, HeadCircumference = 0.5f, AbdominalCircumference = 0.45f, FetalHeartRate = 120 },
new FetalGrowthStandard { Week = 7, Gender = 1, GestationalAge = "7 weeks", MinWeight = 0.12f, MaxWeight = 0.15f, AverageWeight = 0.135f, MinHeight = 0.4f, MaxHeight = 0.45f, AverageHeight = 0.425f, HeadCircumference = 0.55f, AbdominalCircumference = 0.5f, FetalHeartRate = 125 },
new FetalGrowthStandard { Week = 8, Gender = 1, GestationalAge = "8 weeks", MinWeight = 0.8f, MaxWeight = 1.2f, AverageWeight = 1.0f, MinHeight = 1.4f, MaxHeight = 1.8f, AverageHeight = 1.6f, HeadCircumference = 0.6f, AbdominalCircumference = 0.55f, FetalHeartRate = 125 },
new FetalGrowthStandard { Week = 9, Gender = 1, GestationalAge = "9 weeks", MinWeight = 1.6f, MaxWeight = 2.4f, AverageWeight = 2.0f, MinHeight = 2.1f, MaxHeight = 2.5f, AverageHeight = 2.3f, HeadCircumference = 0.65f, AbdominalCircumference = 0.6f, FetalHeartRate = 125 },
new FetalGrowthStandard { Week = 10, Gender = 1, GestationalAge = "10 weeks", MinWeight = 3.2f, MaxWeight = 4.8f, AverageWeight = 4.0f, MinHeight = 2.9f, MaxHeight = 3.3f, AverageHeight = 3.1f, HeadCircumference = 0.7f, AbdominalCircumference = 0.65f, FetalHeartRate = 130 },
new FetalGrowthStandard { Week = 11, Gender = 1, GestationalAge = "11 weeks", MinWeight = 5.6f, MaxWeight = 8.4f, AverageWeight = 7.0f, MinHeight = 3.9f, MaxHeight = 4.3f, AverageHeight = 4.1f, HeadCircumference = 0.75f, AbdominalCircumference = 0.7f, FetalHeartRate = 130 },
new FetalGrowthStandard { Week = 12, Gender = 1, GestationalAge = "12 weeks", MinWeight = 11.2f, MaxWeight = 16.8f, AverageWeight = 14.0f, MinHeight = 5.2f, MaxHeight = 5.6f, AverageHeight = 5.4f, HeadCircumference = 0.8f, AbdominalCircumference = 0.75f, FetalHeartRate = 130 },
new FetalGrowthStandard { Week = 13, Gender = 1, GestationalAge = "13 weeks", MinWeight = 18.4f, MaxWeight = 27.6f, AverageWeight = 23.0f, MinHeight = 7.2f, MaxHeight = 7.6f, AverageHeight = 7.4f, HeadCircumference = 0.85f, AbdominalCircumference = 0.8f, FetalHeartRate = 135 },
new FetalGrowthStandard { Week = 14, Gender = 1, GestationalAge = "14 weeks", MinWeight = 34.4f, MaxWeight = 51.6f, AverageWeight = 43.0f, MinHeight = 8.5f, MaxHeight = 8.9f, AverageHeight = 8.7f, HeadCircumference = 0.9f, AbdominalCircumference = 0.85f, FetalHeartRate = 135 },
new FetalGrowthStandard { Week = 15, Gender = 1, GestationalAge = "15 weeks", MinWeight = 56.0f, MaxWeight = 84.0f, AverageWeight = 70.0f, MinHeight = 9.9f, MaxHeight = 10.3f, AverageHeight = 10.1f, HeadCircumference = 0.95f, AbdominalCircumference = 0.9f, FetalHeartRate = 135 },
new FetalGrowthStandard { Week = 16, Gender = 1, GestationalAge = "16 weeks", MinWeight = 80.0f, MaxWeight = 120.0f, AverageWeight = 100.0f, MinHeight = 11.4f, MaxHeight = 11.8f, AverageHeight = 11.6f, HeadCircumference = 1.0f, AbdominalCircumference = 0.95f, FetalHeartRate = 135 },
new FetalGrowthStandard { Week = 17, Gender = 1, GestationalAge = "17 weeks", MinWeight = 112.0f, MaxWeight = 168.0f, AverageWeight = 140.0f, MinHeight = 12.8f, MaxHeight = 13.2f, AverageHeight = 13.0f, HeadCircumference = 1.05f, AbdominalCircumference = 1.0f, FetalHeartRate = 140 },
new FetalGrowthStandard { Week = 18, Gender = 1, GestationalAge = "18 weeks", MinWeight = 152.0f, MaxWeight = 228.0f, AverageWeight = 190.0f, MinHeight = 14.0f, MaxHeight = 14.4f, AverageHeight = 14.2f, HeadCircumference = 1.1f, AbdominalCircumference = 1.05f, FetalHeartRate = 140 },
new FetalGrowthStandard { Week = 19, Gender = 1, GestationalAge = "19 weeks", MinWeight = 192.0f, MaxWeight = 288.0f, AverageWeight = 240.0f, MinHeight = 15.1f, MaxHeight = 15.5f, AverageHeight = 15.3f, HeadCircumference = 1.15f, AbdominalCircumference = 1.1f, FetalHeartRate = 140 },
new FetalGrowthStandard { Week = 20, Gender = 1, GestationalAge = "20 weeks", MinWeight = 240.0f, MaxWeight = 360.0f, AverageWeight = 300.0f, MinHeight = 16.2f, MaxHeight = 16.6f, AverageHeight = 16.4f, HeadCircumference = 1.2f, AbdominalCircumference = 1.15f, FetalHeartRate = 140 },
new FetalGrowthStandard { Week = 21, Gender = 1, GestationalAge = "21 weeks", MinWeight = 288.0f, MaxWeight = 432.0f, AverageWeight = 360.0f, MinHeight = 25.4f, MaxHeight = 25.8f, AverageHeight = 25.6f, HeadCircumference = 1.25f, AbdominalCircumference = 1.2f, FetalHeartRate = 140 },
new FetalGrowthStandard { Week = 22, Gender = 1, GestationalAge = "22 weeks", MinWeight = 344.0f, MaxWeight = 516.0f, AverageWeight = 430.0f, MinHeight = 27.6f, MaxHeight = 28.0f, AverageHeight = 27.8f, HeadCircumference = 1.3f, AbdominalCircumference = 1.25f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 23, Gender = 1, GestationalAge = "23 weeks", MinWeight = 400.8f, MaxWeight = 601.2f, AverageWeight = 501.0f, MinHeight = 28.7f, MaxHeight = 29.1f, AverageHeight = 28.9f, HeadCircumference = 1.35f, AbdominalCircumference = 1.3f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 24, Gender = 1, GestationalAge = "24 weeks", MinWeight = 480.0f, MaxWeight = 720.0f, AverageWeight = 600.0f, MinHeight = 29.8f, MaxHeight = 30.2f, AverageHeight = 30.0f, HeadCircumference = 1.4f, AbdominalCircumference = 1.35f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 25, Gender = 1, GestationalAge = "25 weeks", MinWeight = 528.0f, MaxWeight = 792.0f, AverageWeight = 660.0f, MinHeight = 34.4f, MaxHeight = 34.8f, AverageHeight = 34.6f, HeadCircumference = 1.45f, AbdominalCircumference = 1.4f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 26, Gender = 1, GestationalAge = "26 weeks", MinWeight = 608.0f, MaxWeight = 912.0f, AverageWeight = 760.0f, MinHeight = 35.4f, MaxHeight = 35.8f, AverageHeight = 35.6f, HeadCircumference = 1.5f, AbdominalCircumference = 1.45f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 27, Gender = 1, GestationalAge = "27 weeks", MinWeight = 700.0f, MaxWeight = 1050.0f, AverageWeight = 875.0f, MinHeight = 36.4f, MaxHeight = 36.8f, AverageHeight = 36.6f, HeadCircumference = 1.55f, AbdominalCircumference = 1.5f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 28, Gender = 1, GestationalAge = "28 weeks", MinWeight = 804.0f, MaxWeight = 1206.0f, AverageWeight = 1005.0f, MinHeight = 37.4f, MaxHeight = 37.8f, AverageHeight = 37.6f, HeadCircumference = 1.6f, AbdominalCircumference = 1.55f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 29, Gender = 1, GestationalAge = "29 weeks", MinWeight = 922.4f, MaxWeight = 1383.6f, AverageWeight = 1153.0f, MinHeight = 38.4f, MaxHeight = 38.8f, AverageHeight = 38.6f, HeadCircumference = 1.65f, AbdominalCircumference = 1.6f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 30, Gender = 1, GestationalAge = "30 weeks", MinWeight = 1055.2f, MaxWeight = 1582.8f, AverageWeight = 1319.0f, MinHeight = 39.7f, MaxHeight = 40.1f, AverageHeight = 39.9f, HeadCircumference = 1.7f, AbdominalCircumference = 1.65f, FetalHeartRate = 145 },
new FetalGrowthStandard { Week = 31, Gender = 1, GestationalAge = "31 weeks", MinWeight = 1201.6f, MaxWeight = 1802.4f, AverageWeight = 1502.0f, MinHeight = 40.9f, MaxHeight = 41.3f, AverageHeight = 41.1f, HeadCircumference = 1.75f, AbdominalCircumference = 1.7f, FetalHeartRate = 150 },
new FetalGrowthStandard { Week = 32, Gender = 1, GestationalAge = "32 weeks", MinWeight = 1361.6f, MaxWeight = 2042.4f, AverageWeight = 1702.0f, MinHeight = 42.2f, MaxHeight = 42.6f, AverageHeight = 42.4f, HeadCircumference = 1.8f, AbdominalCircumference = 1.75f, FetalHeartRate = 150 },
new FetalGrowthStandard { Week = 33, Gender = 1, GestationalAge = "33 weeks", MinWeight = 1534.4f, MaxWeight = 2301.6f, AverageWeight = 1918.0f, MinHeight = 43.5f, MaxHeight = 43.9f, AverageHeight = 43.7f, HeadCircumference = 1.85f, AbdominalCircumference = 1.8f, FetalHeartRate = 155 },
new FetalGrowthStandard { Week = 34, Gender = 1, GestationalAge = "34 weeks", MinWeight = 1716.8f, MaxWeight = 2575.2f, AverageWeight = 2146.0f, MinHeight = 44.8f, MaxHeight = 45.2f, AverageHeight = 45.0f, HeadCircumference = 1.9f, AbdominalCircumference = 1.85f, FetalHeartRate = 155 },
new FetalGrowthStandard { Week = 35, Gender = 1, GestationalAge = "35 weeks", MinWeight = 1906.4f, MaxWeight = 2859.6f, AverageWeight = 2383.0f, MinHeight = 46.0f, MaxHeight = 46.4f, AverageHeight = 46.2f, HeadCircumference = 1.95f, AbdominalCircumference = 1.9f, FetalHeartRate = 155 },
new FetalGrowthStandard { Week = 36, Gender = 1, GestationalAge = "36 weeks", MinWeight = 2097.6f, MaxWeight = 3146.4f, AverageWeight = 2622.0f, MinHeight = 47.2f, MaxHeight = 47.6f, AverageHeight = 47.4f, HeadCircumference = 2.0f, AbdominalCircumference = 1.95f, FetalHeartRate = 155 },
new FetalGrowthStandard { Week = 37, Gender = 1, GestationalAge = "37 weeks", MinWeight = 2287.2f, MaxWeight = 3430.8f, AverageWeight = 2859.0f, MinHeight = 48.4f, MaxHeight = 48.8f, AverageHeight = 48.6f, HeadCircumference = 2.05f, AbdominalCircumference = 2.0f, FetalHeartRate = 155 },
new FetalGrowthStandard { Week = 38, Gender = 1, GestationalAge = "38 weeks", MinWeight = 2466.4f, MaxWeight = 3699.6f, AverageWeight = 3083.0f, MinHeight = 49.6f, MaxHeight = 50.0f, AverageHeight = 49.8f, HeadCircumference = 2.1f, AbdominalCircumference = 2.05f, FetalHeartRate = 160 },
new FetalGrowthStandard { Week = 39, Gender = 1, GestationalAge = "39 weeks", MinWeight = 2630.4f, MaxWeight = 3945.6f, AverageWeight = 3288.0f, MinHeight = 50.5f, MaxHeight = 50.9f, AverageHeight = 50.7f, HeadCircumference = 2.15f, AbdominalCircumference = 2.1f, FetalHeartRate = 160 },
new FetalGrowthStandard { Week = 40, Gender = 1, GestationalAge = "40 weeks", MinWeight = 2769.6f, MaxWeight = 4154.4f, AverageWeight = 3462.0f, MinHeight = 51.0f, MaxHeight = 51.4f, AverageHeight = 51.2f, HeadCircumference = 2.2f, AbdominalCircumference = 2.15f, FetalHeartRate = 160 },
new FetalGrowthStandard { Week = 41, Gender = 1, GestationalAge = "41 weeks", MinWeight = 2799.6f, MaxWeight = 4254.4f, AverageWeight = 3492.0f, MinHeight = 51.5f, MaxHeight = 51.5f, AverageHeight = 51.5f, HeadCircumference = 2.25f, AbdominalCircumference = 2.2f, FetalHeartRate = 160 },


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
