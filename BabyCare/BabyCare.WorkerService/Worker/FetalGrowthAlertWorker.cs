using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core.Utils;
using BabyCare.Repositories.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BabyCare.WorkerService.Worker
{
    public class FetalGrowthAlertWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<FetalGrowthAlertWorker> _logger;
        // Thời gian kiểm tra: ví dụ mỗi 1 phút 
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public FetalGrowthAlertWorker(IServiceScopeFactory serviceScopeFactory, ILogger<FetalGrowthAlertWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FetalGrowthAlertWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking fetal growth records for alerts...");

                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                        var membershipPackageService = scope.ServiceProvider.GetRequiredService<IMembershipPackageService>();

                        await CreateAlertsAndSendEmails(dbContext,membershipPackageService, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking fetal growth alerts.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("FetalGrowthAlertWorker is stopping.");
        }

        private async Task CreateAlertsAndSendEmails(DatabaseContext dbContext, IMembershipPackageService membershipPackageService, CancellationToken stoppingToken)
        {
            // Lấy các FetalGrowthRecord chưa có Alert, bao gồm chuẩn phát triển và thông tin trẻ (với thông tin người dùng)
            var records = await dbContext.FetalGrowthRecords
                .Include(r => r.FetalGrowthStandard)
                .Include(r => r.Child)
                    .ThenInclude(c => c.User)
                .Where(r => r.Alert == null)
                .ToListAsync(); // ❌ Lỗi 1: Không dùng `stoppingToken`

            foreach (var record in records)
            {
                if (record.FetalGrowthStandard == null)
                {
                    _logger.LogWarning("FetalGrowthRecord {RecordId} does not có FetalGrowthStandard.", record.Id);
                    continue;
                }
                if (record.Child == null)
                {
                    _logger.LogWarning("FetalGrowthRecord {RecordId} does not có Child.", record.Id);
                    continue;
                }
                if (await membershipPackageService.HasStandardDeviationAlerts(record.Child.UserId) == false)
                {
                    continue;
                }


                // Kiểm tra cân nặng và chiều cao theo khoảng min - max
                bool isUnderweight = record.Weight < record.FetalGrowthStandard.MinWeight;
                bool isOverweight = record.Weight > record.FetalGrowthStandard.MaxWeight;
                bool isUnderHeight = record.Height < record.FetalGrowthStandard.MinHeight;
                bool isOverHeight = record.Height > record.FetalGrowthStandard.MaxHeight;

                // Kiểm tra vòng đầu (±10%)
                bool isLowHeadCircumference = record.HeadCircumference < record.FetalGrowthStandard.HeadCircumference * 0.9f;
                bool isHighHeadCircumference = record.HeadCircumference > record.FetalGrowthStandard.HeadCircumference * 1.1f;

                // Kiểm tra tần số tim thai (±10%) nếu cả hai có giá trị
                bool isLowFetalHeartRate = false;
                bool isHighFetalHeartRate = false;
                if (record.FetalHeartRate.HasValue && record.FetalGrowthStandard.FetalHeartRate.HasValue)
                {
                    int expectedFHR = record.FetalGrowthStandard.FetalHeartRate.Value;
                    isLowFetalHeartRate = record.FetalHeartRate.Value < expectedFHR * 0.9;
                    isHighFetalHeartRate = record.FetalHeartRate.Value > expectedFHR * 1.1;
                }

                // Kiểm tra vòng bụng (AbdominalCircumference) với ±10%
                bool isLowAbdominalCircumference = record.AbdominalCircumference < record.FetalGrowthStandard.AbdominalCircumference * 0.9f;
                bool isHighAbdominalCircumference = record.AbdominalCircumference > record.FetalGrowthStandard.AbdominalCircumference * 1.1f;

                // Nếu bất kỳ chỉ số nào nằm ngoài giới hạn, tạo Alert
                if (isUnderweight || isOverweight || isUnderHeight || isOverHeight ||
                    isLowHeadCircumference || isHighHeadCircumference ||
                    isLowFetalHeartRate || isHighFetalHeartRate ||
                    isLowAbdominalCircumference || isHighAbdominalCircumference)
                {
                    string alertDescription = string.Empty;
                    if (isUnderweight)
                        alertDescription += $"Cân nặng ({record.Weight} kg) thấp hơn chuẩn tối thiểu ({record.FetalGrowthStandard.MinWeight} kg).<br>";
                    if (isOverweight)
                        alertDescription += $"Cân nặng ({record.Weight} kg) vượt quá chuẩn tối đa ({record.FetalGrowthStandard.MaxWeight} kg).<br>";
                    if (isUnderHeight)
                        alertDescription += $"Chiều cao ({record.Height} cm) thấp hơn chuẩn tối thiểu ({record.FetalGrowthStandard.MinHeight} cm).<br>";
                    if (isOverHeight)
                        alertDescription += $"Chiều cao ({record.Height} cm) vượt quá chuẩn tối đa ({record.FetalGrowthStandard.MaxHeight} cm).<br>";
                    if (isLowHeadCircumference)
                        alertDescription += $"Vòng đầu ({record.HeadCircumference} cm) thấp hơn chuẩn ({record.FetalGrowthStandard.HeadCircumference} cm ±10%).<br>";
                    if (isHighHeadCircumference)
                        alertDescription += $"Vòng đầu ({record.HeadCircumference} cm) vượt quá chuẩn ({record.FetalGrowthStandard.HeadCircumference} cm ±10%).<br>";
                    if (isLowFetalHeartRate)
                        alertDescription += $"Tần số tim thai ({record.FetalHeartRate.Value} bpm) thấp hơn chuẩn ({record.FetalGrowthStandard.FetalHeartRate.Value} bpm ±10%).<br>";
                    if (isHighFetalHeartRate)
                        alertDescription += $"Tần số tim thai ({record.FetalHeartRate.Value} bpm) vượt quá chuẩn ({record.FetalGrowthStandard.FetalHeartRate.Value} bpm ±10%).<br>";
                    if (isLowAbdominalCircumference)
                        alertDescription += $"Vòng bụng ({record.AbdominalCircumference} cm) thấp hơn chuẩn ({record.FetalGrowthStandard.AbdominalCircumference} cm ±10%).<br>";
                    if (isHighAbdominalCircumference)
                        alertDescription += $"Vòng bụng ({record.AbdominalCircumference} cm) vượt quá chuẩn ({record.FetalGrowthStandard.AbdominalCircumference} cm ±10%).<br>";

                    var alert = new Alert
                    {
                        RecordId = record.Id,
                        Title = $"Cảnh báo phát triển thai nhi - Tuần {record.WeekOfPregnancy}",
                        Description = alertDescription,
                        IsRead = false,
                        Type = "FetalGrowth",
                        DateAlerted = DateTime.Now,
                        Record = record
                    };

                    dbContext.Alerts.Add(alert);
                    _logger.LogInformation("Created alert for FetalGrowthRecord {RecordId}:\n{Description}", record.Id, alertDescription);

                    // Gửi email cảnh báo nếu có email của người dùng liên quan đến trẻ
                    var recipientEmail = record.Child?.User?.Email; // ❌ Lỗi 2: Không kiểm tra record.Child trước khi truy cập Name
                    if (!string.IsNullOrEmpty(recipientEmail))
                    {
                        string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FormSendEmail", "Alert.html");
                        templatePath = Path.GetFullPath(templatePath);

                        if (!File.Exists(templatePath))
                        {
                            _logger.LogError("Không tìm thấy template email: {Path}", templatePath);
                        }
                        else
                        {
                            string content = await File.ReadAllTextAsync(templatePath);

                            // Giữ nguyên format gốc và thay thế biến
                            content = content.Replace("{{ChildName}}", record.Child.Name)
                                     .Replace("{{WeekOfPregnancy}}", record.WeekOfPregnancy.ToString())
                                     .Replace("{{Weight}}", record.Weight.ToString("N3"))
                                     .Replace("{{MinWeight}}", record.FetalGrowthStandard.MinWeight.ToString("N3"))
                                     .Replace("{{MaxWeight}}", record.FetalGrowthStandard.MaxWeight.ToString("N3"))
                                     .Replace("{{Height}}", record.Height.ToString("N3"))
                                     .Replace("{{MinHeight}}", record.FetalGrowthStandard.MinHeight.ToString("N3"))
                                     .Replace("{{MaxHeight}}", record.FetalGrowthStandard.MaxHeight.ToString("N3"))
                                     .Replace("{{HeadCircumference}}", record.HeadCircumference.Value.ToString("N3"))
                                     .Replace("{{StandardHeadCircumference}}", record.FetalGrowthStandard.HeadCircumference.ToString("N3"))
                                     .Replace("{{AbdominalCircumference}}", record.AbdominalCircumference.Value.ToString("N3"))
                                     .Replace("{{StandardAbdominalCircumference}}", record.FetalGrowthStandard.AbdominalCircumference.ToString("N3"))
                                     .Replace("{{FetalHeartRate}}", record.FetalHeartRate.HasValue ? record.FetalHeartRate.Value.ToString("N3") : "N/A")
                                     .Replace("{{StandardFetalHeartRate}}", record.FetalGrowthStandard.FetalHeartRate.HasValue ? record.FetalGrowthStandard.FetalHeartRate.Value.ToString("N3") : "N/A")
                                     .Replace("{{RecordedAt}}", record.RecordedAt.ToString("yyyy-MM-dd HH:mm"))
                                     .Replace("{{AlertDescription}}", alertDescription);

                            bool emailSent = DoingMail.SendMail("BabyCare", "Cảnh báo phát triển thai nhi", content, recipientEmail);
                            if (emailSent)
                            {
                                _logger.LogInformation("Email cảnh báo đã được gửi đến {Email} cho FetalGrowthRecord {RecordId}.", recipientEmail, record.Id);
                            }
                            else
                            {
                                _logger.LogError("Gửi email thất bại cho {Email}", recipientEmail);
                            }
                        }
                    }
                }
            }

            await dbContext.SaveChangesAsync(); // ❌ Lỗi 3: Đáng lẽ gọi 1 lần sau vòng lặp, nhưng bị gọi sau mỗi lần gửi mail.
        }


    }
}
