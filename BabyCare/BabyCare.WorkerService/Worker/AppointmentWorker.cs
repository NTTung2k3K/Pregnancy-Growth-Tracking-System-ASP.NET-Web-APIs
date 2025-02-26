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
                _logger.LogInformation("Checking and creating reminders...");

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
                    _logger.LogError(ex, "Error while checking reminders.");
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private async Task CheckCancelAppointment(DatabaseContext dbContext)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;

            // Khai báo thời gian cho từng slot
            var slotTimes = new Dictionary<int, TimeSpan>
            {
                { 1, new TimeSpan(9, 30, 0) },  
                { 2, new TimeSpan(12, 0, 0) },  
                { 3, new TimeSpan(14, 30, 0) }, 
                { 4, new TimeSpan(17, 0, 0) }   
            };

            var appointments = await dbContext.Appointments
                .Where(a => a.Status == (int)BabyCare.Core.Utils.SystemConstant.AppointmentStatus.Pending &&
                            (a.AppointmentDate < today ||
                             (a.AppointmentDate == today && slotTimes.ContainsKey(a.AppointmentSlot) && now.TimeOfDay > slotTimes[a.AppointmentSlot])))
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                appointment.Status = (int)BabyCare.Core.Utils.SystemConstant.AppointmentStatus.CancelledByUser;
            }

            await dbContext.SaveChangesAsync();
        }


    }
}
