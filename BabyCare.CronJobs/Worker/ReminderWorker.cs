using BabyCare.Contract.Repositories.Entity;
using BabyCare.Core.Utils;
using BabyCare.Repositories.Context;
using BabyCare.Services.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BabyCare.CronJobs.Worker
{
    public class ReminderWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ReminderWorker> _logger;

        public ReminderWorker(IServiceScopeFactory serviceScopeFactory, ILogger<ReminderWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking and creating reminders...");

                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                        await CreateAndSendReminders(dbContext);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking reminders.");
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private async Task CreateAndSendReminders(DatabaseContext dbContext)
        {
            Console.WriteLine("Cronjob CreateAndSendReminders is runningg...");

            var tomorrow = DateTime.UtcNow.AddDays(1).Date;

            // Lấy danh sách Appointment có Status là "Confirm", chưa có Reminder và diễn ra vào ngày mai
            var appointments = await dbContext.Appointments
                .Where(a => a.Status == (int)BabyCare.Core.Utils.SystemConstant.AppointmentStatus.Confirmed &&
                            a.AppointmentDate.Date == tomorrow &&
                            !dbContext.Reminders.Any(r => r.AppointmentId == a.Id))
                .Include(a => a.AppointmentUsers)
                    .ThenInclude(au => au.User)
                .Include(a => a.AppointmentChildren)
                    .ThenInclude(ac => ac.Child)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                var reminder = new Reminder
                {
                    AppointmentId = appointment.Id,
                    ReminderType = "Email",
                    ReminderDate = DateTime.UtcNow,
                    CreatedBy = "Admin",
                    IsSent = false
                };

                dbContext.Reminders.Add(reminder);
                await dbContext.SaveChangesAsync(); // Lưu Reminder mới vào DB

                foreach (var user in appointment.AppointmentUsers.Select(au => au.User))
                {
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        // Tải template email
                        string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FormSendEmail", "Reminder.html");
                        templatePath = Path.GetFullPath(templatePath);

                        if (!File.Exists(templatePath))
                        {
                            _logger.LogError("Không tìm thấy template email: {Path}", templatePath);
                            continue;
                        }

                        string content = await File.ReadAllTextAsync(templatePath);

                        // Danh sách trẻ liên quan
                        string childInfo = "";
                        foreach (var ac in appointment.AppointmentChildren)
                        {
                            var child = ac.Child;
                            if (child != null)
                            {
                                string gender = child.FetalGender == 0 ? "Nam" : child.FetalGender == 1 ? "Nữ" : "Chưa xác định";
                                childInfo += $"- {child.Name ?? "N/A"}, Giới tính: {gender}, Nhóm máu: {child.BloodType?.ToString() ?? "N/A"} <br>";
                            }
                        }

                        // Thay thế dữ liệu vào template
                        content = content.Replace("{{Name}}", user.FullName)
                                         .Replace("{{AppointmentName}}", appointment.Name)
                                         .Replace("{{AppointmentDate}}", appointment.AppointmentDate.ToString("yyyy-MM-dd"))
                                         .Replace("{{AppointmentSlot}}", AppointmentService.GetSlotString(appointment.AppointmentSlot))
                                         .Replace("{{Notes}}", appointment.Notes ?? "Không có")
                                         .Replace("{{Description}}", appointment.Description ?? "Không có")
                                         .Replace("{{Fee}}", appointment.Fee?.ToString("N0") ?? "Miễn phí")
                                         .Replace("{{ChildrenInfo}}", string.IsNullOrWhiteSpace(childInfo) ? "Không có trẻ em liên quan" : childInfo);

                        bool emailSent = DoingMail.SendMail("BabyCare", "Appointment Reminder", content, user.Email);

                        if (emailSent)
                        {
                            reminder.IsSent = true;
                            reminder.SentDate = DateTime.UtcNow;
                        }
                        else
                        {
                            _logger.LogError("Failed to send email to {Email}", user.Email);
                        }
                    }
                }

                await dbContext.SaveChangesAsync(); // Lưu trạng thái IsSent của Reminder
            }
        }




    }

}
