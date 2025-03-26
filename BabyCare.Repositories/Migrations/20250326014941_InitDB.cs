using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace BabyCare.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class InitDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AppointmentTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    DaysFromBirth = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: false),
                    Image = table.Column<string>(type: "longtext", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentTemplates", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BlogTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Thumbnail = table.Column<string>(type: "longtext", nullable: true),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogTypes", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FetalGrowthStandards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Week = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    GestationalAge = table.Column<string>(type: "longtext", nullable: true),
                    MinWeight = table.Column<float>(type: "float", nullable: false),
                    MaxWeight = table.Column<float>(type: "float", nullable: false),
                    AverageWeight = table.Column<float>(type: "float", nullable: false),
                    MinHeight = table.Column<float>(type: "float", nullable: false),
                    MaxHeight = table.Column<float>(type: "float", nullable: false),
                    AverageHeight = table.Column<float>(type: "float", nullable: false),
                    HeadCircumference = table.Column<float>(type: "float", nullable: false),
                    AbdominalCircumference = table.Column<float>(type: "float", nullable: false),
                    FetalHeartRate = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FetalGrowthStandards", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MembershipPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PackageName = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    OriginalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    PackageLevel = table.Column<int>(type: "int", nullable: true),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ShowPriority = table.Column<int>(type: "int", nullable: true),
                    MaxRecordAdded = table.Column<int>(type: "int", nullable: false),
                    MaxAppointmentCanBooking = table.Column<int>(type: "int", nullable: false),
                    MaxGrowthChartShares = table.Column<int>(type: "int", nullable: false),
                    HasGenerateAppointments = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasStandardDeviationAlerts = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasViewGrowthChart = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipPackages", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    FullName = table.Column<string>(type: "longtext", nullable: true),
                    Image = table.Column<string>(type: "longtext", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Address = table.Column<string>(type: "longtext", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    BloodGroup = table.Column<string>(type: "longtext", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true),
                    LastUpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    RefreshToken = table.Column<string>(type: "longtext", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true),
                    SecurityStamp = table.Column<string>(type: "longtext", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AppointmentTemplateId = table.Column<int>(type: "int", nullable: false),
                    AppointmentSlot = table.Column<int>(type: "int", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Result = table.Column<string>(type: "longtext", nullable: true),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_AppointmentTemplates_AppointmentTemplateId",
                        column: x => x.AppointmentTemplateId,
                        principalTable: "AppointmentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false),
                    ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.UserId, x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "longtext", nullable: false),
                    Content = table.Column<string>(type: "longtext", nullable: false),
                    Week = table.Column<int>(type: "int", nullable: true),
                    AuthorId = table.Column<Guid>(type: "char(36)", nullable: false),
                    LikesCount = table.Column<int>(type: "int", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    Sources = table.Column<string>(type: "longtext", nullable: true),
                    Thumbnail = table.Column<string>(type: "longtext", nullable: false),
                    BlogTypeId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blogs_BlogTypes_BlogTypeId",
                        column: x => x.BlogTypeId,
                        principalTable: "BlogTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Blogs_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Childs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    FetalGender = table.Column<int>(type: "int", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PhotoUrl = table.Column<string>(type: "longtext", nullable: true),
                    BloodType = table.Column<string>(type: "longtext", nullable: true),
                    IsGenerateSampleAppointments = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Childs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Childs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserMemberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    GrowthChartShareCount = table.Column<int>(type: "int", nullable: false),
                    AppointmentBookingCount = table.Column<int>(type: "int", nullable: false),
                    AddedRecordCount = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMemberships_MembershipPackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "MembershipPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMemberships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AppointmentUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    AssignedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AssignedBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Reason = table.Column<string>(type: "longtext", nullable: true),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentUsers_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppointmentUsers_Users_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppointmentUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    ReminderType = table.Column<string>(type: "longtext", nullable: true),
                    ReminderDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsSent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reminders_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AppointmentChild",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    ChildId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentChild", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentChild_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppointmentChild_Childs_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Childs",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FetalGrowthRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ChildId = table.Column<int>(type: "int", nullable: false),
                    WeekOfPregnancy = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<float>(type: "float", nullable: false),
                    Height = table.Column<float>(type: "float", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FetalGrowthStandardId = table.Column<int>(type: "int", nullable: true),
                    HeadCircumference = table.Column<float>(type: "float", nullable: true),
                    AbdominalCircumference = table.Column<float>(type: "float", nullable: true),
                    FetalHeartRate = table.Column<int>(type: "int", nullable: true),
                    HealthCondition = table.Column<string>(type: "longtext", nullable: true),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FetalGrowthRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FetalGrowthRecords_Childs_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Childs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FetalGrowthRecords_FetalGrowthStandards_FetalGrowthStandardId",
                        column: x => x.FetalGrowthStandardId,
                        principalTable: "FetalGrowthStandards",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GrowthCharts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Question = table.Column<string>(type: "longtext", nullable: false),
                    Topic = table.Column<string>(type: "longtext", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    ChildId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrowthCharts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrowthCharts_Childs_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Childs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MembershipId = table.Column<int>(type: "int", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "longtext", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_UserMemberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "UserMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RecordId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Type = table.Column<string>(type: "longtext", nullable: true),
                    DateAlerted = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_FetalGrowthRecords_RecordId",
                        column: x => x.RecordId,
                        principalTable: "FetalGrowthRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false),
                    GrowthChartsID = table.Column<int>(type: "int", nullable: false),
                    ResponseFeedbackId = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    FeedbackType = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsAnonymous = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Feedbacks_ResponseFeedbackId",
                        column: x => x.ResponseFeedbackId,
                        principalTable: "Feedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedbacks_GrowthCharts_GrowthChartsID",
                        column: x => x.GrowthChartsID,
                        principalTable: "GrowthCharts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_RecordId",
                table: "Alerts",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentChild_AppointmentId",
                table: "AppointmentChild",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentChild_ChildId",
                table: "AppointmentChild",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentTemplateId",
                table: "Appointments",
                column: "AppointmentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentUsers_AppointmentId",
                table: "AppointmentUsers",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentUsers_DoctorId",
                table: "AppointmentUsers",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentUsers_UserId",
                table: "AppointmentUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_AuthorId",
                table: "Blogs",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_BlogTypeId",
                table: "Blogs",
                column: "BlogTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Childs_UserId",
                table: "Childs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_GrowthChartsID",
                table: "Feedbacks",
                column: "GrowthChartsID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ResponseFeedbackId",
                table: "Feedbacks",
                column: "ResponseFeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_UserId",
                table: "Feedbacks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FetalGrowthRecords_ChildId",
                table: "FetalGrowthRecords",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_FetalGrowthRecords_FetalGrowthStandardId",
                table: "FetalGrowthRecords",
                column: "FetalGrowthStandardId");

            migrationBuilder.CreateIndex(
                name: "IX_GrowthCharts_ChildId",
                table: "GrowthCharts",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MembershipId",
                table: "Payments",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_AppointmentId",
                table: "Reminders",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMemberships_PackageId",
                table: "UserMemberships",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMemberships_UserId",
                table: "UserMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "AppointmentChild");

            migrationBuilder.DropTable(
                name: "AppointmentUsers");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "FetalGrowthRecords");

            migrationBuilder.DropTable(
                name: "BlogTypes");

            migrationBuilder.DropTable(
                name: "GrowthCharts");

            migrationBuilder.DropTable(
                name: "UserMemberships");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "FetalGrowthStandards");

            migrationBuilder.DropTable(
                name: "Childs");

            migrationBuilder.DropTable(
                name: "MembershipPackages");

            migrationBuilder.DropTable(
                name: "AppointmentTemplates");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
