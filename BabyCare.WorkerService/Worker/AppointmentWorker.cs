using BabyCare.Contract.Repositories.Entity;
using BabyCare.Core.Utils;
using BabyCare.Repositories.Context;
using BabyCare.Services.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.WorkerService.Worker
{
    public class AppointmentWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AppointmentWorker> _logger;

        public AppointmentWorker(IServiceScopeFactory serviceScopeFactory, ILogger<AppointmentWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking Appointment...");

                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                        await CheckCancelAppointment(dbContext);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking appointments.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task CheckCancelAppointment(DatabaseContext dbContext)
        {
            var now = DateTime.Now;
            var today = now.Date;

            // Lấy danh sách các Appointment cần kiểm tra từ database trước
            var appointments = await dbContext.Appointments
                .Where(a => a.Status == (int)BabyCare.Core.Utils.SystemConstant.AppointmentStatus.Pending)
                .ToListAsync(); // ⚡ Chuyển thành danh sách trên bộ nhớ

            // Danh sách chứa (slotId, thời gian bắt đầu của slot)
            var slotTimes = new List<Tuple<int, TimeSpan>>
            {
                Tuple.Create(1, new TimeSpan(9, 30, 0)),
                Tuple.Create(2, new TimeSpan(12, 0, 0)),
                Tuple.Create(3, new TimeSpan(14, 30, 0)),
                Tuple.Create(4, new TimeSpan(17, 0, 0))
            };

            // Danh sách để lưu các appointment cần hủy
            var filteredAppointments = new List<Appointment>();

            foreach (var a in appointments)
            {
                if (a.AppointmentDate < today)
                {
                    filteredAppointments.Add(a);
                }
                else if (a.AppointmentDate.Date == today)
                {
                    // Tìm xem slot có tồn tại không
                    var slot = slotTimes.FirstOrDefault(s => s.Item1 == a.AppointmentSlot);
                    if (slot != null && now.TimeOfDay > slot.Item2) // Kiểm tra thời gian
                    {
                        filteredAppointments.Add(a);
                    }
                }
            }

            // Cập nhật trạng thái các lịch hẹn đã bị hủy
            if (filteredAppointments.Any()) // Chỉ cập nhật nếu có dữ liệu
            {
                foreach (var appointment in filteredAppointments)
                {
                    appointment.Status = (int)BabyCare.Core.Utils.SystemConstant.AppointmentStatus.CancelledByUser;
                }

                await dbContext.SaveChangesAsync();
            }
        }



    }
}
