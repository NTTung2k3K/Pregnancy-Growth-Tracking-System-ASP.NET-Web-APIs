﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BabyCare.Contract.Repositories.Entity;
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
                        await CreateAlertsAndSendEmails(dbContext, stoppingToken);
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

        private async Task CreateAlertsAndSendEmails(DatabaseContext dbContext, CancellationToken stoppingToken)
        {
            // Lấy các FetalGrowthRecord chưa có Alert, bao gồm chuẩn phát triển và thông tin trẻ (với thông tin người dùng)
            var records = await dbContext.FetalGrowthRecords
                .Include(r => r.FetalGrowthStandard)
                .Include(r => r.Child)
                    .ThenInclude(c => c.User)
                .Where(r => r.Alert == null)
                .ToListAsync(stoppingToken);

            foreach (var record in records)
            {
                if (record.FetalGrowthStandard == null)
                {
                    _logger.LogWarning("FetalGrowthRecord {RecordId} does not have an associated FetalGrowthStandard.", record.Id);
                    continue;
                }
                if (record.Child == null)
                {
                    _logger.LogWarning("FetalGrowthRecord {RecordId} does not have an associated Child.", record.Id);
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
                        alertDescription += $"Cân nặng ({record.Weight} kg) thấp hơn chuẩn tối thiểu ({record.FetalGrowthStandard.MinWeight} kg). ";
                    if (isOverweight)
                        alertDescription += $"Cân nặng ({record.Weight} kg) vượt quá chuẩn tối đa ({record.FetalGrowthStandard.MaxWeight} kg). ";
                    if (isUnderHeight)
                        alertDescription += $"Chiều cao ({record.Height} cm) thấp hơn chuẩn tối thiểu ({record.FetalGrowthStandard.MinHeight} cm). ";
                    if (isOverHeight)
                        alertDescription += $"Chiều cao ({record.Height} cm) vượt quá chuẩn tối đa ({record.FetalGrowthStandard.MaxHeight} cm). ";
                    if (isLowHeadCircumference)
                        alertDescription += $"Vòng đầu ({record.HeadCircumference} cm) thấp hơn chuẩn ({record.FetalGrowthStandard.HeadCircumference} cm ±10%). ";
                    if (isHighHeadCircumference)
                        alertDescription += $"Vòng đầu ({record.HeadCircumference} cm) vượt quá chuẩn ({record.FetalGrowthStandard.HeadCircumference} cm ±10%). ";
                    if (isLowFetalHeartRate)
                        alertDescription += $"Tần số tim thai ({record.FetalHeartRate.Value} bpm) thấp hơn chuẩn ({record.FetalGrowthStandard.FetalHeartRate.Value} bpm ±10%). ";
                    if (isHighFetalHeartRate)
                        alertDescription += $"Tần số tim thai ({record.FetalHeartRate.Value} bpm) vượt quá chuẩn ({record.FetalGrowthStandard.FetalHeartRate.Value} bpm ±10%). ";
                    if (isLowAbdominalCircumference)
                        alertDescription += $"Vòng bụng ({record.AbdominalCircumference} cm) thấp hơn chuẩn ({record.FetalGrowthStandard.AbdominalCircumference} cm ±10%). ";
                    if (isHighAbdominalCircumference)
                        alertDescription += $"Vòng bụng ({record.AbdominalCircumference} cm) vượt quá chuẩn ({record.FetalGrowthStandard.AbdominalCircumference} cm ±10%). ";

                    var alert = new Alert
                    {
                        RecordId = record.Id,
                        Title = $"Cảnh báo phát triển thai nhi - Tuần {record.WeekOfPregnancy}",
                        Description = alertDescription,
                        IsRead = false,
                        Type = "FetalGrowth",
                        DateAlerted = DateTime.UtcNow,
                        Record = record
                    };

                    dbContext.Alerts.Add(alert);
                    _logger.LogInformation("Created alert for FetalGrowthRecord {RecordId}: {Description}", record.Id, alertDescription);

                    // Gửi email cảnh báo nếu có email của người dùng liên quan đến trẻ
                    var recipientEmail = record.Child.User?.Email;
                    if (!string.IsNullOrEmpty(recipientEmail))
                    {
                        // Đường dẫn tới template Alert.html
                        string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FormSendEmail", "Alert.html");
                        templatePath = Path.GetFullPath(templatePath);

                        if (!File.Exists(templatePath))
                        {
                            _logger.LogError("Không tìm thấy template email: {Path}", templatePath);
                        }
                        else
                        {
                            string content = await File.ReadAllTextAsync(templatePath, stoppingToken);
                            content = content.Replace("{{ChildName}}", record.Child.Name)
                                             .Replace("{{WeekOfPregnancy}}", record.WeekOfPregnancy.ToString())
                                             .Replace("{{Weight}}", record.Weight.ToString("N1"))
                                             .Replace("{{MinWeight}}", record.FetalGrowthStandard.MinWeight.ToString("N1"))
                                             .Replace("{{MaxWeight}}", record.FetalGrowthStandard.MaxWeight.ToString("N1"))
                                             .Replace("{{Height}}", record.Height.ToString("N1"))
                                             .Replace("{{MinHeight}}", record.FetalGrowthStandard.MinHeight.ToString("N1"))
                                             .Replace("{{MaxHeight}}", record.FetalGrowthStandard.MaxHeight.ToString("N1"))
                                             .Replace("{{HeadCircumference}}", record.HeadCircumference.Value.ToString("N1"))
                                             .Replace("{{StandardHeadCircumference}}", record.FetalGrowthStandard.HeadCircumference.ToString("N1"))
                                             .Replace("{{AbdominalCircumference}}", record.AbdominalCircumference.Value.ToString("N1"))
                                             .Replace("{{StandardAbdominalCircumference}}", record.FetalGrowthStandard.AbdominalCircumference.ToString("N1"))
                                             .Replace("{{FetalHeartRate}}", record.FetalHeartRate.HasValue ? record.FetalHeartRate.Value.ToString() : "N/A")
                                             .Replace("{{StandardFetalHeartRate}}", record.FetalGrowthStandard.FetalHeartRate.HasValue ? record.FetalGrowthStandard.FetalHeartRate.Value.ToString() : "N/A")
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
                    else
                    {
                        _logger.LogWarning("Không có email của người dùng liên quan đến trẻ {ChildName}.", record.Child.Name);
                    }
                }
            }

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}
