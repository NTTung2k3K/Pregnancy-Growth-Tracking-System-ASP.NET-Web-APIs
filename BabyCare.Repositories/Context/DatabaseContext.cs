﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BabyCare.Contract.Repositories.Entity;


namespace BabyCare.Repositories.Context
{
    public class DatabaseContext : IdentityDbContext<ApplicationUsers, ApplicationRoles, Guid, ApplicationUserClaims, ApplicationUserRoles, ApplicationUserLogins, ApplicationRoleClaims, ApplicationUserTokens>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        // user
        public virtual DbSet<ApplicationUsers> ApplicationUsers => Set<ApplicationUsers>();
        public virtual DbSet<ApplicationRoles> ApplicationRoles => Set<ApplicationRoles>();
        public virtual DbSet<ApplicationUserClaims> ApplicationUserClaims => Set<ApplicationUserClaims>();
        public virtual DbSet<ApplicationUserRoles> ApplicationUserRoles => Set<ApplicationUserRoles>();
        public virtual DbSet<ApplicationUserLogins> ApplicationUserLogins => Set<ApplicationUserLogins>();
        public virtual DbSet<ApplicationRoleClaims> ApplicationRoleClaims => Set<ApplicationRoleClaims>();
        public virtual DbSet<ApplicationUserTokens> ApplicationUserTokens => Set<ApplicationUserTokens>();


        // Domain-specific tables
        public virtual DbSet<Appointment> Appointments => Set<Appointment>();

        public virtual DbSet<UserMessage> UserMessages => Set<UserMessage>();
        public virtual DbSet<AppointmentUser> AppointmentUsers => Set<AppointmentUser>();

        public virtual DbSet<Reminder> Reminders => Set<Reminder>();
        public virtual DbSet<BlogType> BlogTypes => Set<BlogType>();
        public virtual DbSet<Blog> Blogs => Set<Blog>();
        public virtual DbSet<MembershipPackage> MembershipPackages => Set<MembershipPackage>();
        public virtual DbSet<UserMembership> UserMemberships => Set<UserMembership>();
        public virtual DbSet<Payment> Payments => Set<Payment>();
        public virtual DbSet<Child> Children => Set<Child>();
        public virtual DbSet<FetalGrowthStandard> FetalGrowthStandards => Set<FetalGrowthStandard>();
        public virtual DbSet<GrowthChart> GrowthCharts => Set<GrowthChart>();
        public virtual DbSet<FetalGrowthRecord> FetalGrowthRecords => Set<FetalGrowthRecord>();
        public virtual DbSet<Alert> Alerts => Set<Alert>();
        public virtual DbSet<Feedback> Feedbacks => Set<Feedback>();
        public virtual DbSet<AppointmentChild> AppointmentChildren => Set<AppointmentChild>();
        public virtual DbSet<AppointmentTemplates> AppointmentTemplates => Set<AppointmentTemplates>();


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseLazyLoadingProxies();
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            /*builder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.Doctor)
                    .WithMany()
                    .HasForeignKey(a => a.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict); // Disable cascading delete for Doctor
            });*/

            //builder.Entity<AppointmentUser>()
            //.HasKey(au => au.Id); // Đặt Id làm khóa chính

            //builder.Entity<AppointmentUser>()
            //    .Property(au => au.Id)
            //    .ValueGeneratedOnAdd()
            //    .UseIdentityColumn(); // Giá trị được sinh tự động khi thêm


            builder.Entity<AppointmentUser>(entity =>
            {
                //// Primary Key
                //entity.HasKey(e => e.Id);

                //// Identity Column (Auto-Increment)
                //entity.Property(e => e.Id)
                //    .ValueGeneratedOnAdd();

                // Foreign Key - Doctor (Nullable)
                entity.HasOne(e => e.Doctor)
                    .WithMany()
                    .HasForeignKey(e => e.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);




            });



            //builder.Entity<ApplicationUsers>()
            //    .HasMany(u => u.AppointmentUsers) // Một ApplicationUsers có nhiều AppointmentUsers
            //    .WithOne(au => au.User)          // Một AppointmentUser liên kết với một ApplicationUser
            //    .HasForeignKey(au => au.UserId)  // Khóa ngoại UserId
            //    .OnDelete(DeleteBehavior.Restrict); // Hành vi khi xóa // Định nghĩa hành vi xóa

            //// Cấu hình quan hệ giữa DoctorId trong AppointmentUser với ApplicationUsers
            //builder.Entity<AppointmentUser>()
            //    .HasOne(au => au.Doctor)
            //    .WithMany()
            //    .HasForeignKey(au => au.DoctorId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //// Cấu hình quan hệ giữa AppointmentId trong AppointmentUser với Appointment
            //builder.Entity<AppointmentUser>()
            //    .HasOne(au => au.Appointment)
            //    .WithMany()
            //    .HasForeignKey(au => au.Id)
            //    .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Feedback>().HasOne(x => x.User).WithMany(x => x.Feedbacks).HasForeignKey(x => x.UserId).IsRequired().OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Feedback>().HasOne(x => x.GrowthChart).WithMany(x => x.Feedbacks).HasForeignKey(x => x.GrowthChartsID).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Feedback>().HasOne(x => x.ResponseFeedback)
                .WithMany(x => x.ResponseFeedbacks)
                .HasForeignKey(x => x.ResponseFeedbackId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUserLogins>()
                .HasKey(login => new { login.UserId, login.LoginProvider, login.ProviderKey });
            builder.Entity<ApplicationUserRoles>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });
                userRole.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });

            builder.Entity<ApplicationUserTokens>()
                .HasKey(token => new { token.UserId, token.LoginProvider, token.Name });


            // Apply entity configurations for fluent API setup
            builder.Entity<ApplicationUsers>().ToTable("Users");
            builder.Entity<ApplicationRoles>().ToTable("Roles");
            builder.Entity<ApplicationUserRoles>().ToTable("UserRoles");
            builder.Entity<AppointmentUser>().ToTable("AppointmentUsers");

            // Define table configurations for other entities if needed
            builder.Entity<Appointment>().ToTable("Appointments");
            builder.Entity<UserMessage>().ToTable("UserMessage");

            builder.Entity<Reminder>().ToTable("Reminders");
            builder.Entity<BlogType>().ToTable("BlogTypes");
            builder.Entity<Blog>().ToTable("Blogs");
            builder.Entity<MembershipPackage>().ToTable("MembershipPackages");
            builder.Entity<UserMembership>().ToTable("UserMemberships");
            builder.Entity<Payment>().ToTable("Payments");
            builder.Entity<Child>().ToTable("Childs");
            builder.Entity<FetalGrowthStandard>().ToTable("FetalGrowthStandards");
            builder.Entity<GrowthChart>().ToTable("GrowthCharts");
            builder.Entity<FetalGrowthRecord>().ToTable("FetalGrowthRecords");
            builder.Entity<Alert>().ToTable("Alerts");
            builder.Entity<Feedback>().ToTable("Feedbacks");
            builder.Entity<AppointmentChild>().ToTable("AppointmentChild");
        }
    }
}
