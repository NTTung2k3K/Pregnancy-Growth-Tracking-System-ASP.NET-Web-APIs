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
                .AddScoped<IChildService, ChildService>()
                .AddScoped<IVnpay, Vnpay>()
                .AddHttpContextAccessor()
                .AddScoped<IMembershipPackageService, MembershipPackageService>()
                .AddScoped<IAppointmentTemplateService, AppointmentTemplateService>();


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
                    EmailConfirmed = true,
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
                    Email = "doctor@example.com",
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };

                var result = await userManager.CreateAsync(doctorUser, "Doctor1@");

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
                    EmailConfirmed = true,
                    LockoutEnabled = false
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
        Name = "Khám thai lần đầu",
        DaysFromBirth = -270,
        Description = "Kiểm tra xác nhận thai, tính tuổi thai và đưa ra dự sinh.",
        Status = 1,
        Image = "https://cdn-icons-png.flaticon.com/512/3209/3209960.png",
        Fee = 100000 // 100.000đ
    },
    new AppointmentTemplates
    {
        Name = "Siêu âm lần 1",
        DaysFromBirth = -210,
        Description = "Siêu âm để kiểm tra hình thái học của thai nhi lần đầu.",
        Status = 1,
        Image = "https://cdn-icons-png.flaticon.com/512/1989/1989553.png",
        Fee = 150000 // 150.000đ
    },
    new AppointmentTemplates
    {
        Name = "Xét nghiệm máu lần đầu",
        DaysFromBirth = -180,
        Description = "Xét nghiệm máu để kiểm tra nguy cơ dị tật di truyền hoặc bất thường.",
        Status = 1,
        Image = "https://cdn-icons-png.flaticon.com/512/1055/1055672.png",
        Fee = 200000 // 200.000đ
    },
    new AppointmentTemplates
    {
        Name = "Siêu âm dị tật thai nhi",
        DaysFromBirth = -120,
        Description = "Siêu âm chi tiết để phát hiện dị tật bẩm sinh hoặc vấn đề bất thường.",
        Status = 1,
        Image = "https://cdn-icons-png.flaticon.com/512/3209/3209929.png",
        Fee = 250000 // 250.000đ
    },
    new AppointmentTemplates
    {
        Name = "Xét nghiệm đường huyết",
        DaysFromBirth = -90,
        Description = "Kiểm tra đường huyết để phát hiện tiểu đường thai kỳ.",
        Status = 1,
        Image = "https://cdn-icons-png.flaticon.com/512/2580/2580426.png",
        Fee = 300000 // 300.000đ
    },
    new AppointmentTemplates
    {
        Name = "Tiêm phòng uốn ván lần 1",
        DaysFromBirth = -60,
        Description = "Tiêm phòng uốn ván để bảo vệ mẹ và thai nhi.",
        Status = 1,
        Image = "https://cdn-icons-png.flaticon.com/512/4210/4210947.png",
        Fee = 50000 // 50.000đ
    },
    new AppointmentTemplates
    {
        Name = "Siêu âm theo dõi sự phát triển",
        DaysFromBirth = -30,
        Description = "Siêu âm để đánh giá sự phát triển của thai nhi (cân nặng, nước ối).",
        Status = 1,
        Image = "https://cdn-icons-png.flaticon.com/512/3209/3209934.png",
        Fee = 200000 // 200.000đ
    },
    new AppointmentTemplates
    {
        Name = "Kiểm tra thai kỳ cuối",
        DaysFromBirth = -7,
        Description = "Kiểm tra sức khỏe mẹ và thai nhi trước khi sinh.",
        Status = 1,
        Image = "https://cdn-icons-png.flaticon.com/512/3209/3209970.png",
        Fee = 100000 // 100.000đ
    },
    new AppointmentTemplates
    {
        Name = "Kiểm tra sau sinh",
        DaysFromBirth = 30,
        Description = "Kiểm tra tình trạng sức khỏe mẹ và bé sau sinh.",
        Status = 1,
        Image = "https://cdn-icons-png.flaticon.com/512/2254/2254821.png",
        Fee = 120000 // 120.000đ
    }
};
                dbContext.AppointmentTemplates.AddRange(templates);
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
