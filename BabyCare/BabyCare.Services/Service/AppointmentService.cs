using AutoMapper;
using Azure.Core;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.Core.Base;
using BabyCare.Core.Firebase;
using BabyCare.Core.Utils;
using BabyCare.ModelViews.AppointmentModelViews.Request;
using BabyCare.ModelViews.AppointmentModelViews.Response;
using BabyCare.ModelViews.AppointmentTemplateModelViews.Response;
using BabyCare.ModelViews.AuthModelViews.Response;
using BabyCare.ModelViews.ChildModelView;
using BabyCare.ModelViews.FetalGrowthRecordModelView;
using BabyCare.ModelViews.UserModelViews.Response;
using BabyCare.Repositories.Context;
using Firebase.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BabyCare.Services.Service
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUsers> _userManager;
        private readonly IFetalGrowthRecordService _fetalGrowthRecordService;
        private readonly DatabaseContext _context;




        public AppointmentService(DatabaseContext context,IFetalGrowthRecordService fetalGrowthRecordService, IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor, UserManager<ApplicationUsers> userManager)
        {
            _context = context;
            _fetalGrowthRecordService = fetalGrowthRecordService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }
        public async Task<Guid?> GetRandomDoctorIdAsync()
        {
            var doctors = (await _userManager.GetUsersInRoleAsync(SystemConstant.Role.DOCTOR)).ToList();

            // Kiểm tra nếu danh sách trống
            if (doctors == null || !doctors.Any())
            {
                return null;
            }

            // Lấy ngẫu nhiên một bác sĩ
            var random = new Random();
            var randomDoctor = doctors[random.Next(doctors.Count)];

            // Trả về ID của bác sĩ ngẫu nhiên
            return randomDoctor.Id;
        }

        public async Task<ApiResult<object>> CreateAppointment(CreateAppointmentRequest request)
        {
            if (request.IsDoctorCreate)
            {

                if (_contextAccessor.HttpContext?.User?.FindFirst("userId") == null)
                {
                    return new ApiErrorResult<object>("Plase login to use this function.", System.Net.HttpStatusCode.BadRequest);
                }
            }
            var repoAppointment = _unitOfWork.GetRepository<Appointment>();
            var repoAppointmentUser = _unitOfWork.GetRepository<AppointmentUser>();
            var repoChild = _unitOfWork.GetRepository<Child>();
            var repoAT = _unitOfWork.GetRepository<AppointmentTemplates>();
            var repoAC = _unitOfWork.GetRepository<AppointmentChild>();




            // Check user is existed
            var existingUser = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (existingUser == null)
            {
                return new ApiErrorResult<object>("User is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Check child is existed
            //var existingChild = await repoChild.GetByIdAsync(request.ChildId);
            //if (existingChild == null)
            //{
            //    return new ApiErrorResult<object>("Child is not existed.", System.Net.HttpStatusCode.NotFound);
            //}
            // Check appointment template is existed
            var existingAT = await repoAT.GetByIdAsync(request.AppointmentTemplateId);
            if (existingAT == null)
            {
                return new ApiErrorResult<object>("Appointment Type is not existed.", System.Net.HttpStatusCode.NotFound);
            }



            try
            {
                _unitOfWork.BeginTransaction();
                var doctorId = request.IsDoctorCreate ? Guid.Parse(_contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value) : await GetRandomDoctorIdAsync();

                // Check available slot
                if (!request.IsDoctorCreate)
                {
                    var slotOnDay = repoAppointment.Entities
                        .Where(x => x.AppointmentDate.Date == request.AppointmentDate.Date
                                && x.AppointmentUsers.Any(u => u.DoctorId == doctorId))
                        .ToList();

                    if (slotOnDay.Count >= SystemConstant.MAX_SLOT_AVAILABLE_APPOINTMENT)
                    {
                        // Nếu doctor được chọn đã full slot, lấy danh sách doctor khác để tìm slot trống
                        var availableDoctor = _userManager.Users
                            .Where(d => !repoAppointment.Entities
                                .Any(a => a.AppointmentDate.Date == request.AppointmentDate.Date
                                       && a.AppointmentSlot == request.AppointmentSlot
                                       && a.AppointmentUsers.Any(u => u.DoctorId == d.Id)))
                            .FirstOrDefault();

                        if (availableDoctor != null)
                        {
                            doctorId = availableDoctor.Id; // Gán doctor mới có slot trống
                        }
                        else
                        {
                            return new ApiErrorResult<object>(
                                "Day " + request.AppointmentDate.Date.ToShortDateString() + " is full of slot. Please choose another day!",
                                System.Net.HttpStatusCode.NotFound
                            );
                        }
                    }

                    // Kiểm tra lại nếu slot vẫn bị trùng thì báo lỗi
                    if (slotOnDay.Any(x => x.AppointmentSlot == request.AppointmentSlot))
                    {
                        return new ApiErrorResult<object>(
                            "Slot has been selected by another doctor. Please choose another slot.",
                            System.Net.HttpStatusCode.NotFound
                        );
                    }
                }

                var appointment = new Appointment()
                {
                    AppointmentTemplateId = request.AppointmentTemplateId,
                    AppointmentSlot = request.AppointmentSlot,
                    AppointmentDate = request.AppointmentDate,
                    Status = (int)AppointmentStatus.Confirmed,
                    Fee = existingAT.Fee * request.ChildIds.Count,
                    Notes = request.Notes,
                    Name = request.Name,
                    Description = request.Description,
                };
                await repoAppointment.InsertAsync(appointment);
                await repoAppointment.SaveAsync();
                await _unitOfWork.SaveAsync();
                //var auId = await GetNextAppointmentUserIdAsync();
                var appointmentUser = new AppointmentUser()
                {
                    //Id = auId,
                    AppointmentId = appointment.Id,
                    Appointment = appointment,
                    CreatedTime = DateTime.Now,
                    Description = $"{existingAT.Name}_{appointment.AppointmentDate.ToString()}_Slot:{appointment.AppointmentSlot}_Estimate Cost:{appointment.Fee}",
                    UserId = request.UserId,
                    DoctorId = doctorId != null ? doctorId.Value : null,
                    AssignedBy = request.IsDoctorCreate ? Guid.Parse(_contextAccessor.HttpContext?.User?.FindFirst("userId").Value) : request.UserId,
                    AssignedTime = DateTime.Now,
                };
                await repoAppointmentUser.InsertAsync(appointmentUser);
                await repoAppointmentUser.SaveAsync();

                foreach (var childId in request.ChildIds)
                {
                    var existingChild = await repoChild.GetByIdAsync(childId);
                    if (existingChild == null)
                    {
                        return new ApiErrorResult<object>("Child is not existed.", System.Net.HttpStatusCode.NotFound);
                    }
                    var appointmentChild = new AppointmentChild()
                    {
                        Appointment = appointment,
                        Child = existingChild,
                        Description = $"{existingChild.Name}_{request.Name}",

                    };
                    await repoAC.InsertAsync(appointmentChild);
                }

                await repoAC.SaveAsync();


                _unitOfWork.CommitTransaction();

                // Send mail for user
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FormSendEmail", "CustomerForm.html");
                path = Path.GetFullPath(path);
                if (!File.Exists(path))
                {
                    return new ApiSuccessResult<object>("Booking successfully, but not found form to send mail");
                }
                var content = File.ReadAllText(path);
                content = content.Replace("{{CustomerName}}", appointment.AppointmentUsers.FirstOrDefault().User.FullName);
                content = content.Replace("{{AppointmentDate}}", appointment.AppointmentDate.Date.ToString());
                content = content.Replace("{{AppointmentTime}}", GetSlotString(appointment.AppointmentSlot));
                content = content.Replace("{{ServiceName}}", appointment.AppointmentTemplate.Name);
                content = content.Replace("{{Description}}", appointment.AppointmentTemplate.Description);
                content = content.Replace("{{Fee}}", appointment.AppointmentTemplate.Fee?.ToString("N0", new System.Globalization.CultureInfo("vi-VN")) + "VNĐ");
                content = content.Replace("{{PhoneNumber}}", appointment.AppointmentUsers.FirstOrDefault().User.PhoneNumber);
                content = content.Replace("{{Email}}", appointment.AppointmentUsers.FirstOrDefault().User.Email);
                string childrenHtml = "";
                foreach (var child in appointment.AppointmentChildren)
                {
                    childrenHtml += $@"
        <tr>
            <td style='padding: 12px; text-align: center;'>{child.Child.Name}</td>
            <td style='padding: 12px; text-align: center;'>{child.Child.PregnancyWeekAtBirth}</td>
            <td style='padding: 12px; text-align: center;'>{child.Child.DueDate:dd/MM/yyyy}</td>
        </tr>";
                }

                content = content.Replace("{{childs}}", childrenHtml);

                var resultSendMail = DoingMail.SendMail("BabyCare", "Booking successfully", content, appointment.AppointmentUsers.FirstOrDefault().User.Email);
                if (!resultSendMail)
                {
                    return new ApiErrorResult<object>("Cannot send email to " + appointment.AppointmentUsers.FirstOrDefault().User.Email);
                }
                // Send mail to admin
                string pathAdmin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FormSendEmail", "ManagerForm.html");
                pathAdmin = Path.GetFullPath(pathAdmin);
                if (!File.Exists(pathAdmin))
                {
                    return new ApiSuccessResult<object>("Booking successfully, but not found form to send mail to admin");
                }
                var contentAdmin = File.ReadAllText(pathAdmin);
                contentAdmin = contentAdmin.Replace("{{CustomerName}}", appointment.AppointmentUsers.FirstOrDefault().User.FullName);
                contentAdmin = contentAdmin.Replace("{{AppointmentDate}}", appointment.AppointmentDate.Date.ToString());
                contentAdmin = contentAdmin.Replace("{{AppointmentTime}}", GetSlotString(appointment.AppointmentSlot));
                contentAdmin = contentAdmin.Replace("{{ServiceName}}", appointment.AppointmentTemplate.Name);
                contentAdmin = contentAdmin.Replace("{{Description}}", appointment.AppointmentTemplate.Description);
                contentAdmin = contentAdmin.Replace("{{Fee}}", appointment.AppointmentTemplate.Fee?.ToString("N0", new System.Globalization.CultureInfo("vi-VN")) + "VNĐ");
                contentAdmin = contentAdmin.Replace("{{PhoneNumber}}", appointment.AppointmentUsers.FirstOrDefault().User.PhoneNumber);
                contentAdmin = contentAdmin.Replace("{{Email}}", appointment.AppointmentUsers.FirstOrDefault().User.Email);
                string childrenAdminHtml = "";
                foreach (var child in appointment.AppointmentChildren)
                {
                    childrenAdminHtml += $@"
        <tr>
            <td style='padding: 12px; text-align: center;'>{child.Child.Name}</td>
            <td style='padding: 12px; text-align: center;'>{child.Child.PregnancyWeekAtBirth}</td>
            <td style='padding: 12px; text-align: center;'>{child.Child.DueDate:dd/MM/yyyy}</td>
        </tr>";
                }

                contentAdmin = contentAdmin.Replace("{{childs}}", childrenAdminHtml);

                var doctorEmail = await _userManager.FindByIdAsync(doctorId.ToString());
                if (doctorEmail == null)
                {
                    return new ApiSuccessResult<object>("Appointment confirmed successfully.");

                }

                var resultSendMailAdmin = DoingMail.SendMail("BabyCare", "New Booking", contentAdmin, doctorEmail.Email);
                if (!resultSendMailAdmin)
                {
                    return new ApiErrorResult<object>("Cannot send email to " + doctorEmail.Email);
                }




                return new ApiSuccessResult<object>("Create successfully.");
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new Exception("Transaction failed.", ex);
            }

        }

        public async Task<int> GetNextAppointmentUserIdAsync()
        {
            // Lấy Id lớn nhất từ bảng AppointmentUser
            var maxId = await _unitOfWork.GetRepository<AppointmentUser>().Entities
                .OrderByDescending(a => a.Id)
                .Select(a => a.Id)
                .FirstOrDefaultAsync();

            // Nếu không có Id nào trong database, trả về 1
            return maxId + 1;
        }


        public async Task<ApiResult<object>> DeleteAppointment(DeleteAppointmentRequest request)
        {
            var repo = _unitOfWork.GetRepository<Appointment>();
            // Check mp is existed
            var existingItem = await repo.Entities.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingItem == null)
            {
                return new ApiErrorResult<object>("Appointment is not existed.");
            }
            if (_contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value == null)
            {
                return new ApiErrorResult<object>("Plase login to use this function.", System.Net.HttpStatusCode.BadRequest);
            }
            existingItem.DeletedTime = DateTime.Now;
            existingItem.DeletedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

            await repo.UpdateAsync(existingItem);
            await repo.SaveAsync();

            return new ApiSuccessResult<object>("Delete successfully.");
        }

        public async Task<ApiResult<AppointmentResponseModelV2>> GetAppointmentById(int id)
        {
            var repo = _unitOfWork.GetRepository<Appointment>();
            var repoChild = _unitOfWork.GetRepository<Child>();
            var repoAT = _unitOfWork.GetRepository<AppointmentTemplates>();
            var repoFGR = _unitOfWork.GetRepository<FetalGrowthRecord>();
            var repoFGSD = _unitOfWork.GetRepository<FetalGrowthStandard>();



            // Check mp is existed
            var existingItem = await repo.Entities.Include(x => x.AppointmentTemplate).Include(x => x.AppointmentUsers).ThenInclude(x => x.User)
                .Include(x => x.AppointmentChildren).ThenInclude(x => x.Child)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (existingItem == null || existingItem.DeletedBy != null)
            {
                return new ApiErrorResult<AppointmentResponseModelV2>("Appointment is not existed.");
            }
            var response = _mapper.Map<AppointmentResponseModelV2>(existingItem);


            if (Enum.IsDefined(typeof(AppointmentStatus), existingItem.Status))
            {
                response.Status = ((AppointmentStatus)existingItem.Status).ToString();
            }
            else
            {
                response.Status = "Unknown";
            }
            var user = await _userManager.FindByIdAsync(existingItem.AppointmentUsers.FirstOrDefault().UserId.ToString());

            response.User = _mapper.Map<UserResponseModel>(user);
            response.AppoinmentUsers = new();
            
            var appointmentUser = existingItem.AppointmentUsers.OrderByDescending(x => x.AssignedTime).FirstOrDefault();
            if(appointmentUser != null)
            {
                var appointmentUserModel = _mapper.Map<AppoinmentUserResponseModel>(appointmentUser);
                appointmentUserModel.Doctor = _mapper.Map<EmployeeResponseModel>(appointmentUserModel.Doctor);

                response.AppoinmentUsers.Add(appointmentUserModel);
            }
            
            var at = repoAT.GetById(existingItem.AppointmentTemplateId);
            response.AppointmentTemplate = _mapper.Map<ATResponseModel>(at);
            response.Childs = new();
            foreach (var child in existingItem.AppointmentChildren)
            {
                // Map Child entity sang ChildModelViewAddeRecords
                var childModel = _mapper.Map<ChildModelViewAddeRecords>(child.Child);

                // Lấy các FGR liên quan và map cùng tiêu chuẩn
                var fgrs = await repoFGR.Entities
                    .Include(x => x.FetalGrowthStandard) // Đảm bảo include dữ liệu liên quan
                    .Where(x => x.ChildId == child.ChildId)
                    .ToListAsync();

                // Map danh sách FGR sang ModelView
                childModel.FetalGrowthRecordModelViews = _mapper.Map<List<FetalGrowthRecordModelViewAddedStandards>>(fgrs);

                // Thêm vào response
                response.Childs.Add(childModel);
            }



            return new ApiSuccessResult<AppointmentResponseModelV2>(response);
        }

        public async Task<ApiResult<BasePaginatedList<AppointmentResponseModel>>> GetAppointmentsPagination(BaseSearchRequest request)
        {

            var repoChild = _unitOfWork.GetRepository<Child>();
            var repoAT = _unitOfWork.GetRepository<AppointmentTemplates>();

            var items = _unitOfWork.GetRepository<Appointment>().Entities.Include(x => x.AppointmentUsers).ThenInclude(x => x.User)
                .Include(x => x.AppointmentTemplate)
                .Include(x => x.AppointmentChildren).ThenInclude(x => x.Child)
                .Where(x => x.DeletedBy == null);

            // filter by search 
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var searchValueLower = request.SearchValue.ToLower();  // Chuyển về chữ thường để so sánh không phân biệt chữ hoa chữ thường
                items = items.Where(x => x.AppointmentUsers
                    .Any(au => au.User.FullName.ToLower().Contains(searchValueLower) ||
                               au.User.PhoneNumber.ToLower().Contains(searchValueLower)));
            }
            // paging
            var currentPage = request.PageIndex ?? 1;
            var pageSize = request.PageSize ?? SystemConstant.PAGE_SIZE;
            var total = items.Count();
            var data = await items.Skip((currentPage - 1) * currentPage).Take(pageSize).ToListAsync();
            // calculate total page

            var res = new List<AppointmentResponseModel>();
            foreach (var existingItem in data)
            {
                var added = _mapper.Map<AppointmentResponseModel>(existingItem);


                if (Enum.IsDefined(typeof(AppointmentStatus), existingItem.Status))
                {
                    added.Status = ((AppointmentStatus)existingItem.Status).ToString();
                }
                else
                {
                    added.Status = "Unknown";
                }
                var user = await _userManager.FindByIdAsync(existingItem.AppointmentUsers.FirstOrDefault().UserId.ToString());

                added.User = _mapper.Map<UserResponseModel>(user);
                added.Doctors = new();
                foreach (var doctor in existingItem.AppointmentUsers)
                {
                    if (doctor.Doctor == null)
                    {
                        continue;
                    }
                    var doctorCheck = await _userManager.FindByIdAsync(doctor.DoctorId.ToString());
                    var doctorModel = _mapper.Map<EmployeeResponseModel>(doctorCheck);
                    added.Doctors.Add(doctorModel);
                }
                var at = repoAT.GetById(existingItem.AppointmentTemplateId);
                added.AppointmentTemplate = _mapper.Map<ATResponseModel>(at);
                added.Childs = new();
                foreach (var child in existingItem.AppointmentChildren)
                {
                    var childCheck = repoChild.GetById(child.ChildId);
                    var childModel = _mapper.Map<ChildModelView>(childCheck);
                    added.Childs.Add(childModel);
                }
                res.Add(added);
            }

            var response = new BasePaginatedList<AppointmentResponseModel>(res, total, currentPage, pageSize);
            // return to client
            return new ApiSuccessResult<BasePaginatedList<AppointmentResponseModel>>(response);
        }

        public ApiResult<List<AppointmentStatusResponseModel>> GetAppointmentStatusHandler()
        {
            var statusList = Enum.GetValues(typeof(AppointmentStatus))
                    .Cast<AppointmentStatus>()
                    .Select(status => new AppointmentStatusResponseModel
                    {
                        Id = (int)status,
                        Status = status.ToString()
                    })
                   .Where(x => x.Id != (int)AppointmentStatus.Pending &&
                        x.Id != (int)AppointmentStatus.CancelledByUser
                   )
                    .ToList();


            return new ApiSuccessResult<List<AppointmentStatusResponseModel>>(statusList);
        }

        public async Task<ApiResult<object>> UpdateAppointment(UpdateAppointmentRequest request)
        {

            var repo = _unitOfWork.GetRepository<Appointment>();
            var repoChild = _unitOfWork.GetRepository<Child>();
            var repoAT = _unitOfWork.GetRepository<AppointmentTemplates>();
            var repoAC = _unitOfWork.GetRepository<AppointmentChild>();

            _unitOfWork.BeginTransaction();
            // Check appointment is existed
            var existingItem = await repo.Entities.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingItem == null || existingItem.DeletedBy != null)
            {
                return new ApiErrorResult<object>("Appointment is not existed.");
            }
            if (request.IsDoctorUpdate)
            {
                existingItem.Notes = request.Notes;
                existingItem.Status = request.Status;
                existingItem.Name = request.Name;
                existingItem.Fee = request.Fee;
            }
            else
            {
                if (existingItem.CreatedTime <= DateTime.Now.AddHours(-1))
                {
                    return new ApiErrorResult<object>("Appointment cannot update because out of time.");
                }
                if (existingItem.AppointmentDate.Date != request.AppointmentDate.Date && existingItem.AppointmentSlot != existingItem.AppointmentSlot)
                {
                    var slotOnDay = repo.Entities.Where(x => x.AppointmentDate.Date == request.AppointmentDate.Date);

                    if (slotOnDay.Count() >= SystemConstant.MAX_SLOT_AVAILABLE_APPOINTMENT)
                    {
                        return new ApiErrorResult<object>(
                            $"Day {request.AppointmentDate.Date.ToShortDateString()} is full of slots. Please choose another day!",
                            System.Net.HttpStatusCode.NotFound
                        );
                    }

                    // Kiểm tra xem slot đã được chọn chưa
                    if (slotOnDay.Any(x => x.AppointmentSlot == request.AppointmentSlot))
                    {
                        return new ApiErrorResult<object>(
                            "Slot has been selected by other. Please choose another slot",
                            System.Net.HttpStatusCode.NotFound
                        );
                    }
                }

                var existingChildren = await repoAC.Entities
     .Where(x => x.AppointmentId == existingItem.Id)
     .ToListAsync(); // Lấy danh sách các `AppointmentChild` hiện tại từ DB

                // Lấy danh sách ID của các trẻ hiện có trong DB
                var existingChildIds = existingChildren.Select(x => x.ChildId).ToList();

                // Tìm các `ChildId` cần xóa (có trong danh sách cũ nhưng không có trong yêu cầu mới)
                var childrenToRemove = existingChildren
                    .Where(x => x.ChildId != request.ChildId)
                    .ToList();

                // Nếu yêu cầu mới có `ChildId` không có trong danh sách cũ -> cần thêm mới
                var childToAdd = request.ChildId;
                if (!existingChildIds.Contains(childToAdd))
                {
                    // Tạo `AppointmentChild` mới
                    var newAppointmentChild = new AppointmentChild()
                    {
                        AppointmentId = existingItem.Id,
                        ChildId = childToAdd,
                        Description = $"{request.Name}_{existingItem.Name}",
                    };

                    await repoAC.InsertAsync(newAppointmentChild);
                    await repoAC.SaveAsync();
                }

                // Xóa các `AppointmentChild` không còn hợp lệ
                if (childrenToRemove.Any())
                {
                    repoAC.DeleteRange(childrenToRemove);
                    await repoAC.SaveAsync();
                }

                // Cập nhật thông tin `Appointment`
                existingItem.LastUpdatedTime = DateTime.Now;
                existingItem.LastUpdatedBy = request.UserId.ToString();
                existingItem.Notes = request.Notes;
                existingItem.Status = request.Status;
                existingItem.AppointmentSlot = request.AppointmentSlot;
                existingItem.AppointmentDate = request.AppointmentDate;
                existingItem.AppointmentTemplateId = request.AppointmentTemplateId;
                existingItem.Name = request.Name;

                // Lưu thay đổi vào cơ sở dữ liệu
                await repo.UpdateAsync(existingItem);
                await repo.SaveAsync();

                _unitOfWork.CommitTransaction();
            }
            return new ApiSuccessResult<object>("Appointment updated successfully.");



        }

        public async Task<ApiResult<object>> UpdateCancelAppointmentStatusByUser(CancelAppointmentByUser request)
        {
            var repo = _unitOfWork.GetRepository<Appointment>();
            var repoChild = _unitOfWork.GetRepository<Child>();
            var repoAT = _unitOfWork.GetRepository<AppointmentTemplates>();
            var repoAC = _unitOfWork.GetRepository<AppointmentChild>();

            var existingItem = await repo.Entities.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingItem == null || existingItem.DeletedBy != null)
            {
                return new ApiErrorResult<object>("Appointment is not existed.");
            }
            if (existingItem.AppointmentUsers.FirstOrDefault().UserId != request.UserId)
            {
                return new ApiErrorResult<object>("User is valid to cancel.");
            }
            if (existingItem.CreatedTime <= DateTime.Now.AddHours(-1))
            {
                return new ApiErrorResult<object>("Appointment cannot cancel because out of time.");
            }
            existingItem.Status = (int)SystemConstant.AppointmentStatus.CancelledByUser;
            await repo.UpdateAsync(existingItem);
            await repo.SaveAsync();
            return new ApiSuccessResult<object>("Update successfully");
        }

        public async Task<ApiResult<object>> UpdateNoShowAppointmentStatusByDoctor(NoShowAppointmentByDoctor request)
        {
            var repo = _unitOfWork.GetRepository<Appointment>();


            var existingItem = await repo.Entities.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingItem == null || existingItem.DeletedBy != null)
            {
                return new ApiErrorResult<object>("Appointment is not existed.");
            }
            if (_contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value == null)
            {
                return new ApiErrorResult<object>("Plase login to use this function.", System.Net.HttpStatusCode.BadRequest);
            }
            var userId = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
            var checkValid = await _userManager.FindByIdAsync(userId);
            if (checkValid == null)
            {
                return new ApiErrorResult<object>("User is not existed.");
            }
            var validRole = await _userManager.GetRolesAsync(checkValid);
            if (!validRole.Any(x => x.Contains(SystemConstant.Role.DOCTOR)))
            {
                return new ApiErrorResult<object>("User is not authorization to access.", System.Net.HttpStatusCode.Forbidden);
            }
            existingItem.Status = (int)SystemConstant.AppointmentStatus.NoShow;
            await repo.UpdateAsync(existingItem);
            await repo.SaveAsync();
            return new ApiSuccessResult<object>("Update successfully");
        }

        public async Task<ApiResult<AvailableSlotResponseModel>> GetSlotAvailable(DateTime date)
        {
            var doctorId = _contextAccessor.HttpContext?.User?.FindFirst("userId");
            if (doctorId == null)
            {
                return new ApiErrorResult<AvailableSlotResponseModel>("Plase login to use this function.", System.Net.HttpStatusCode.BadRequest);
            }
            int MAX_SLOT_AVAILABLE_APPOINTMENT = SystemConstant.MAX_SLOT_AVAILABLE_APPOINTMENT;
            var repo = _unitOfWork.GetRepository<AppointmentUser>();
            var doctorRawId = Guid.Parse(doctorId.Value);

            // Lấy danh sách các slot đã được đặt bởi bác sĩ cụ thể vào ngày được chọn
            var slotsBookedOnDay = await repo.Entities
                .Where(x => x.DoctorId == doctorRawId && x.Appointment.AppointmentDate == date && x.Appointment.Status != (int)AppointmentStatus.Pending)
                .Select(x => x.Appointment.AppointmentSlot)
                .ToListAsync();

            // Tạo danh sách tất cả các slot trong ngày
            var allSlots = Enumerable.Range(1, MAX_SLOT_AVAILABLE_APPOINTMENT).ToList();

            // Tìm các slot còn trống
            var availableSlots = allSlots.Except(slotsBookedOnDay).ToList();

            // Tạo phản hồi
            var response = new AvailableSlotResponseModel
            {
                Date = date,
                Slots = availableSlots
            };

            return new ApiSuccessResult<AvailableSlotResponseModel>(response, "Take successfully");
        }

        public async Task<ApiResult<List<AppointmentResponseModel>>> GetAppointmentsByUserId(Guid userId)
        {
            var repo = _unitOfWork.GetRepository<Appointment>();
            var repoChild = _unitOfWork.GetRepository<Child>();
            var repoAT = _unitOfWork.GetRepository<AppointmentTemplates>();

            // Lấy tất cả các Appointment liên quan đến userId
            var allAppointments = await repo.Entities
                .Where(x => x.AppointmentUsers.Any(au => au.UserId == userId) && x.DeletedBy == null)
                 .OrderBy(x => x.AppointmentDate)

                .ToListAsync();
            var responseList = new List<AppointmentResponseModel>();

            if (allAppointments == null || !allAppointments.Any())
            {
                return new ApiSuccessResult<List<AppointmentResponseModel>>(responseList);
            }


            // Duyệt qua từng appointment để ánh xạ dữ liệu
            foreach (var appointment in allAppointments)
            {
                var response = new AppointmentResponseModel
                {
                    Id = appointment.Id,
                    AppointmentDate = appointment.AppointmentDate,
                    Status = Enum.IsDefined(typeof(AppointmentStatus), appointment.Status)
                        ? ((AppointmentStatus)appointment.Status).ToString()
                        : "Unknown",
                };

                // Map User
                var user = await _userManager.FindByIdAsync(appointment.AppointmentUsers.FirstOrDefault()?.UserId.ToString());
                response.User = user != null ? _mapper.Map<UserResponseModel>(user) : null;

                // Map Doctors
                response.Doctors = new();
                foreach (var doctor in appointment.AppointmentUsers)
                {
                    if (doctor.Doctor == null) continue;

                    var doctorCheck = await _userManager.FindByIdAsync(doctor.DoctorId.ToString());
                    if (doctorCheck != null)
                    {
                        var doctorModel = _mapper.Map<EmployeeResponseModel>(doctorCheck);
                        response.Doctors.Add(doctorModel);
                    }
                }



                // Map AppointmentTemplate
                var at = await repoAT.GetByIdAsync(appointment.AppointmentTemplateId);
                response.AppointmentTemplate = at != null ? _mapper.Map<ATResponseModel>(at) : null;

                // Map Childs
                response.Childs = new();
                foreach (var child in appointment.AppointmentChildren)
                {
                    var childCheck = await repoChild.GetByIdAsync(child.ChildId);
                    if (childCheck != null)
                    {
                        var childModel = _mapper.Map<ChildModelView>(childCheck);
                        response.Childs.Add(childModel);
                    }
                }

                responseList.Add(response);
            }

            return new ApiSuccessResult<List<AppointmentResponseModel>>(responseList);
        }

        public async Task<ApiResult<List<AppointmentResponseModel>>> GetAppointmentsByUserIdInRange(Guid userId, DateTime startDay, DateTime endDate)
        {
            var repo = _unitOfWork.GetRepository<Appointment>();
            var repoChild = _unitOfWork.GetRepository<Child>();
            var repoAT = _unitOfWork.GetRepository<AppointmentTemplates>();

            // Lấy tất cả các Appointment liên quan đến userId
            var allAppointments = await repo.Entities
                .Where(x => x.AppointmentUsers.Any(au => au.UserId == userId) && x.DeletedBy == null && x.AppointmentDate.Date >= startDay.Date && x.AppointmentDate.Date <= endDate.Date)
                 .OrderBy(x => x.AppointmentDate)
                .ToListAsync();
            var responseList = new List<AppointmentResponseModel>();

            if (allAppointments == null || !allAppointments.Any())
            {
                return new ApiSuccessResult<List<AppointmentResponseModel>>(responseList);
            }


            // Duyệt qua từng appointment để ánh xạ dữ liệu
            foreach (var appointment in allAppointments)
            {
                var response = new AppointmentResponseModel
                {
                    Id = appointment.Id,
                    AppointmentDate = appointment.AppointmentDate,
                    Status = Enum.IsDefined(typeof(AppointmentStatus), appointment.Status)
                        ? ((AppointmentStatus)appointment.Status).ToString()
                        : "Unknown",
                    AppointmentSlot = appointment.AppointmentSlot,
                    Description = appointment.Description,
                    Fee = appointment.Fee,
                    Name = appointment.Name,
                    Result = appointment.Result,
                    Notes = appointment.Notes,
                };

                // Map User
                var user = await _userManager.FindByIdAsync(appointment.AppointmentUsers.FirstOrDefault()?.UserId.ToString());
                response.User = user != null ? _mapper.Map<UserResponseModel>(user) : null;

                // Map Doctors
                response.Doctors = new();
                foreach (var doctor in appointment.AppointmentUsers)
                {
                    if (doctor.Doctor == null) continue;

                    var doctorCheck = await _userManager.FindByIdAsync(doctor.DoctorId.ToString());
                    if (doctorCheck != null)
                    {
                        var doctorModel = _mapper.Map<EmployeeResponseModel>(doctorCheck);
                        response.Doctors.Add(doctorModel);
                    }
                }

                // Map AppointmentTemplate
                var at = await repoAT.GetByIdAsync(appointment.AppointmentTemplateId);
                response.AppointmentTemplate = at != null ? _mapper.Map<ATResponseModel>(at) : null;

                // Map Childs
                response.Childs = new();
                foreach (var child in appointment.AppointmentChildren)
                {
                    var childCheck = await repoChild.GetByIdAsync(child.ChildId);
                    if (childCheck != null)
                    {
                        var childModel = _mapper.Map<ChildModelView>(childCheck);
                        response.Childs.Add(childModel);
                    }
                }

                responseList.Add(response);
            }

            return new ApiSuccessResult<List<AppointmentResponseModel>>(responseList);
        }

        public async Task<ApiResult<List<AppointmentResponseModel>>> GetAll(Guid doctorId)
        {
            var repo = _unitOfWork.GetRepository<Appointment>();
            var repoChild = _unitOfWork.GetRepository<Child>();
            var repoAT = _unitOfWork.GetRepository<AppointmentTemplates>();

            var appointmentsQuery = repo.Entities
                .Where(x => x.DeletedBy == null && x.Status != (int)AppointmentStatus.Pending && x.AppointmentUsers.Any(x => x.DoctorId == doctorId))
                .OrderBy(x => x.AppointmentDate); // Sắp xếp theo Date tăng dần

            // Thực thi truy vấn
            var allAppointments = await appointmentsQuery.ToListAsync();

            // Ánh xạ kết quả sang ResponseModel
            var responseList = new List<AppointmentResponseModel>();
            foreach (var appointment in allAppointments)
            {
                var response = _mapper.Map<AppointmentResponseModel>(appointment);

                // Chuyển Status thành string
                if (Enum.IsDefined(typeof(AppointmentStatus), appointment.Status))
                {
                    response.Status = ((AppointmentStatus)appointment.Status).ToString();
                }
                else
                {
                    response.Status = "Unknown";
                }

                // Lấy thông tin User
                var user = await _userManager.FindByIdAsync(appointment.AppointmentUsers.FirstOrDefault()?.UserId.ToString());
                response.User = _mapper.Map<UserResponseModel>(user);

                // Lấy thông tin Doctor
                response.Doctors = new();
                foreach (var doctor in appointment.AppointmentUsers)
                {
                    if (doctor.Doctor == null) continue;

                    var doctorCheck = await _userManager.FindByIdAsync(doctor.DoctorId.ToString());
                    var doctorModel = _mapper.Map<EmployeeResponseModel>(doctorCheck);
                    response.Doctors.Add(doctorModel);
                }

                // Lấy thông tin Appointment Template
                var at = await repoAT.GetByIdAsync(appointment.AppointmentTemplateId);
                response.AppointmentTemplate = _mapper.Map<ATResponseModel>(at);

                // Lấy thông tin Child
                response.Childs = new();
                foreach (var child in appointment.AppointmentChildren)
                {
                    var childCheck = await repoChild.GetByIdAsync(child.ChildId);
                    var childModel = _mapper.Map<ChildModelView>(childCheck);
                    response.Childs.Add(childModel);
                }

                responseList.Add(response);
            }

            return new ApiSuccessResult<List<AppointmentResponseModel>>(responseList);
        }

        public async Task<ApiResult<object>> UpdateByDoctorAppointment(UpdateAppointmentByDoctorRequest request)
        {
            var repo = _unitOfWork.GetRepository<Appointment>();
            var repoChild = _unitOfWork.GetRepository<Child>();
            var repoAT = _unitOfWork.GetRepository<AppointmentTemplates>();
            var repoAC = _unitOfWork.GetRepository<AppointmentChild>();

            //_unitOfWork.BeginTransaction();
            // Check appointment is existed
            var existingItem = await repo.Entities.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingItem == null || existingItem.DeletedBy != null)
            {
                return new ApiErrorResult<object>("Appointment is not existed.");
            }
            var doctorId = _contextAccessor.HttpContext?.User?.FindFirst("userId");
            if (doctorId == null)
            {
                return new ApiErrorResult<object>("Plase login to use this function.", System.Net.HttpStatusCode.BadRequest);
            }
            var doctorRawId = Guid.Parse(doctorId.Value);
            //Check existing doctor
            var existingDoctor = await _userManager.FindByIdAsync(doctorRawId.ToString());
            if (existingDoctor == null)
            {
                return new ApiErrorResult<object>("Doctor is not existed.");
            }
            //Check valid doctor
            var validRole = await _userManager.GetRolesAsync(existingDoctor);
            if (!validRole.Any(x => x.Contains(SystemConstant.Role.DOCTOR)))
            {
                return new ApiErrorResult<object>("User is not authorization to access.", System.Net.HttpStatusCode.Forbidden);
            }
            // Update fields
            existingItem.Notes = request.Notes;
            existingItem.Status = request.Status;
            existingItem.Name = request.Name;
            existingItem.Fee = request.Fee;
            existingItem.Description = request.Description;
            existingItem.Result = request.Result;
            existingItem.AppointmentDate = DateTime.Now;
            // Update Childs
            foreach (var item in request.ChildsUpdated)
            {

                var createItem = new CreateFetalGrowthRecordModelView()
                {
                    AbdominalCircumference = item.AbdominalCircumference,
                    HeadCircumference = item.HeadCircumference,
                    ChildId = item.ChildId,
                    FetalHeartRate = item.FetalHeartRate,
                    HealthCondition = item.HealthCondition,
                    Height = item.Height,
                    RecordedAt = DateTime.Now,
                    WeekOfPregnancy = item.WeekOfPregnancy,
                    Weight = item.Weight,
                };
                var resultRecord = await _fetalGrowthRecordService.AddFetalGrowthRecordAsync(createItem);
                if (resultRecord.IsSuccessed == false)
                {
                    //_unitOfWork.RollBack();
                    return new ApiErrorResult<object>(resultRecord.Message);
                }
            }
            await _unitOfWork.SaveAsync();
            //_unitOfWork.CommitTransaction();
            return new ApiSuccessResult<object>("Appointment updated successfully.");

        }
        private string NormalizePropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return propertyName;

            return char.ToUpper(propertyName[0]) + propertyName.Substring(1);
        }
        private bool PropertyExists(string propertyName, Type entityType)
        {
            // Kiểm tra nếu thuộc tính tồn tại trong entity
            var property = entityType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            return property != null;
        }

        public async Task<ApiResult<BasePaginatedList<AppointmentResponseModel>>> GetAppointmentsByUserIdPagination(SearchAppointmentByUserId request)
        {
            var query = _unitOfWork.GetRepository<Appointment>().Entities.AsQueryable();
            query = query.Where(x => x.AppointmentUsers.Any(x => x.UserId == request.userId) && x.Status != (int)AppointmentStatus.Pending);
            // 1. Áp dụng bộ lọc (Filtering)
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                query = query.Where(a => a.Name.ToLower().Contains(request.SearchValue.ToLower()) ||
                                         a.AppointmentChildren.Any(x => x.Child.Name.ToLower().Contains(request.SearchValue.ToLower())));
            }
            if (request.FromDate.HasValue)
            {
                query = query.Where(a => a.AppointmentDate.Date >= request.FromDate.Value.Date);
            }
            if (request.ToDate.HasValue)
            {
                query = query.Where(a => a.AppointmentDate.Date <= request.ToDate.Value.Date);
            }
            if (request.Status != null)
            {
                query = query.Where(a => a.Status == request.Status);
            }

            if (request.SortBy != null)
            {
                // 2. Áp dụng sắp xếp (Sorting)
                var normalizedSortBy = NormalizePropertyName(request.SortBy);
                if (!PropertyExists(normalizedSortBy, typeof(Appointment)))
                {
                    // Nếu không tồn tại, bạn có thể xử lý lỗi, hoặc chọn một thuộc tính mặc định
                    throw new ArgumentException($"Property '{request.SortBy}' does not exist on the Appointment entity.");
                }
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    query = request.IsDescending
           ? query.OrderByDescending(a => EF.Property<object>(a, normalizedSortBy).ToString().ToLower())
           : query.OrderBy(a => EF.Property<object>(a, normalizedSortBy).ToString().ToLower());

                }
            }
            else
            {
                query = query.OrderBy(a => a.AppointmentDate); // Mặc định sắp xếp theo ngày hẹn
            }

            // 3. Tổng số bản ghi
            var totalRecords = await query.CountAsync();
            var currentPage = request.PageIndex ?? 1;
            var pageSize = request.PageSize ?? SystemConstant.PAGE_SIZE;
            var total = await query.CountAsync();
            // 4. Áp dụng phân trang (Pagination)
            var data = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var res = new List<AppointmentResponseModel>();
            foreach (var existingItem in data)
            {
                var added = _mapper.Map<AppointmentResponseModel>(existingItem);


                if (Enum.IsDefined(typeof(AppointmentStatus), existingItem.Status))
                {
                    added.Status = ((AppointmentStatus)existingItem.Status).ToString();
                }
                else
                {
                    added.Status = "Unknown";
                }
                var user = await _userManager.FindByIdAsync(existingItem.AppointmentUsers.FirstOrDefault().UserId.ToString());

                added.User = _mapper.Map<UserResponseModel>(user);
                added.Doctors = new();
                foreach (var doctor in existingItem.AppointmentUsers)
                {
                    if (doctor.Doctor == null)
                    {
                        continue;
                    }
                    var doctorCheck = await _userManager.FindByIdAsync(doctor.DoctorId.ToString());
                    var doctorModel = _mapper.Map<EmployeeResponseModel>(doctorCheck);
                    added.Doctors.Add(doctorModel);
                }
                added.AppointmentTemplate = _mapper.Map<ATResponseModel>(existingItem.AppointmentTemplate);
                added.Childs = new();
                foreach (var child in existingItem.AppointmentChildren)
                {
                    var childModel = _mapper.Map<ChildModelView>(child.Child);
                    added.Childs.Add(childModel);
                }
                res.Add(added);
            }

            var response = new BasePaginatedList<AppointmentResponseModel>(res, total, currentPage, pageSize);
            // return to client
            return new ApiSuccessResult<BasePaginatedList<AppointmentResponseModel>>(response);
        }

        public async Task<ApiResult<object>> ConfirmAppointment(ConfirmAppointment request)
        {
            var repoAppointment = _unitOfWork.GetRepository<Appointment>();

            _unitOfWork.BeginTransaction();
            // Check appointment is existed
            var existingItem = await repoAppointment.Entities.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingItem == null || existingItem.DeletedBy != null)
            {
                return new ApiErrorResult<object>("Appointment is not existed.");
            }
            var doctorId = await GetRandomDoctorIdAsync();
            var slotOnDay = repoAppointment.Entities
                .Where(x => x.AppointmentDate.Date == request.AppointmentDate.Date
                        && x.AppointmentUsers.Any(u => u.DoctorId == doctorId))
                .ToList();

            if (slotOnDay.Count >= SystemConstant.MAX_SLOT_AVAILABLE_APPOINTMENT)
            {
                // Nếu doctor được chọn đã full slot, lấy danh sách doctor khác để tìm slot trống
                var availableDoctor = _userManager.Users
                    .Where(d => !repoAppointment.Entities
                        .Any(a => a.AppointmentDate.Date == request.AppointmentDate.Date
                               && a.AppointmentSlot == request.AppointmentSlot
                               && a.AppointmentUsers.Any(u => u.DoctorId == d.Id)))
                    .FirstOrDefault();

                if (availableDoctor != null)
                {
                    doctorId = availableDoctor.Id; // Gán doctor mới có slot trống
                }
                else
                {
                    return new ApiErrorResult<object>(
                        "Day " + request.AppointmentDate.Date.ToShortDateString() + " is full of slot. Please choose another day!",
                        System.Net.HttpStatusCode.NotFound
                    );
                }
            }

            // Kiểm tra lại nếu slot vẫn bị trùng thì báo lỗi
            if (slotOnDay.Any(x => x.AppointmentSlot == request.AppointmentSlot))
            {
                return new ApiErrorResult<object>(
                    "Slot has been selected by another doctor. Please choose another slot.",
                    System.Net.HttpStatusCode.NotFound
                );
            }



            var au = existingItem.AppointmentUsers.FirstOrDefault(x => x.AppointmentId == existingItem.Id);
            au.DoctorId = doctorId;
            // Cập nhật thông tin `Appointment`
            existingItem.LastUpdatedTime = DateTime.Now;
            existingItem.LastUpdatedBy = request.UserId.ToString();
            existingItem.Notes = request.Notes;
            existingItem.Status = (int)AppointmentStatus.Confirmed;
            existingItem.AppointmentSlot = request.AppointmentSlot;
            existingItem.AppointmentDate = request.AppointmentDate;

            // Lưu thay đổi vào cơ sở dữ liệu
            await repoAppointment.UpdateAsync(existingItem);
            await repoAppointment.SaveAsync();

            // Send mail for user
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FormSendEmail", "CustomerForm.html");
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                return new ApiSuccessResult<object>("Booking successfully, but not found form to send mail");
            }
            var content = File.ReadAllText(path);
            content = content.Replace("{{CustomerName}}", existingItem.AppointmentUsers.FirstOrDefault().User.FullName);
            content = content.Replace("{{AppointmentDate}}", existingItem.AppointmentDate.Date.ToString());
            content = content.Replace("{{AppointmentTime}}", GetSlotString(existingItem.AppointmentSlot));
            content = content.Replace("{{ServiceName}}", existingItem.AppointmentTemplate.Name);
            content = content.Replace("{{Description}}", existingItem.AppointmentTemplate.Description);
            content = content.Replace("{{Fee}}", existingItem.AppointmentTemplate.Fee?.ToString("N0", new System.Globalization.CultureInfo("vi-VN")) + "VNĐ");
            content = content.Replace("{{PhoneNumber}}", existingItem.AppointmentUsers.FirstOrDefault().User.PhoneNumber);
            content = content.Replace("{{Email}}", existingItem.AppointmentUsers.FirstOrDefault().User.Email);
            string childrenHtml = "";
            foreach (var child in existingItem.AppointmentChildren)
            {
                childrenHtml += $@"
        <tr>
            <td style='padding: 12px; text-align: center;'>{child.Child.Name}</td>
            <td style='padding: 12px; text-align: center;'>{child.Child.PregnancyWeekAtBirth}</td>
            <td style='padding: 12px; text-align: center;'>{child.Child.DueDate:dd/MM/yyyy}</td>
        </tr>";
            }

            content = content.Replace("{{childs}}", childrenHtml);

            var resultSendMail = DoingMail.SendMail("BabyCare", "Booking successfully", content, existingItem.AppointmentUsers.FirstOrDefault().User.Email);
            if (!resultSendMail)
            {
                return new ApiErrorResult<object>("Cannot send email to " + existingItem.AppointmentUsers.FirstOrDefault().User.Email);
            }
            // Send mail to admin
            string pathAdmin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FormSendEmail", "ManagerForm.html");
            pathAdmin = Path.GetFullPath(pathAdmin);
            if (!File.Exists(pathAdmin))
            {
                return new ApiSuccessResult<object>("Booking successfully, but not found form to send mail to admin");
            }
            var contentAdmin = File.ReadAllText(pathAdmin);
            contentAdmin = contentAdmin.Replace("{{CustomerName}}", existingItem.AppointmentUsers.FirstOrDefault().User.FullName);
            contentAdmin = contentAdmin.Replace("{{AppointmentDate}}", existingItem.AppointmentDate.Date.ToString());
            contentAdmin = contentAdmin.Replace("{{AppointmentTime}}", GetSlotString(existingItem.AppointmentSlot));
            contentAdmin = contentAdmin.Replace("{{ServiceName}}", existingItem.AppointmentTemplate.Name);
            contentAdmin = contentAdmin.Replace("{{Description}}", existingItem.AppointmentTemplate.Description);
            contentAdmin = contentAdmin.Replace("{{Fee}}", existingItem.AppointmentTemplate.Fee?.ToString("N0", new System.Globalization.CultureInfo("vi-VN")) + "VNĐ");
            contentAdmin = contentAdmin.Replace("{{PhoneNumber}}", existingItem.AppointmentUsers.FirstOrDefault().User.PhoneNumber);
            contentAdmin = contentAdmin.Replace("{{Email}}", existingItem.AppointmentUsers.FirstOrDefault().User.Email);
            string childrenAdminHtml = "";
            foreach (var child in existingItem.AppointmentChildren)
            {
                childrenAdminHtml += $@"
        <tr>
            <td style='padding: 12px; text-align: center;'>{child.Child.Name}</td>
            <td style='padding: 12px; text-align: center;'>{child.Child.PregnancyWeekAtBirth}</td>
            <td style='padding: 12px; text-align: center;'>{child.Child.DueDate:dd/MM/yyyy}</td>
        </tr>";
            }

            contentAdmin = contentAdmin.Replace("{{childs}}", childrenAdminHtml);

            var doctorEmail = await _userManager.FindByIdAsync(doctorId.ToString());
            if (doctorEmail == null)
            {
                return new ApiSuccessResult<object>("Appointment confirmed successfully.");

            }

            var resultSendMailAdmin = DoingMail.SendMail("BabyCare", "New Booking", contentAdmin, doctorEmail.Email);
            if (!resultSendMailAdmin)
            {
                return new ApiErrorResult<object>("Cannot send email to " + doctorEmail.Email);
            }


            _unitOfWork.CommitTransaction();
            return new ApiSuccessResult<object>("Appointment confirmed successfully.");
        }
        public static string GetSlotString(int slot)
        {
            if (slot < 1 || slot > 4)
            {
                throw new ArgumentException("Slot number must be between 1 and 4");
            }

            string[] slotTimes = { "07:00 - 09:30", "09:30 - 12:00", "12:00 - 14:30", "14:30 - 17:00" };
            return $"Slot {slot} - {slotTimes[slot - 1]}";
        }

        public async Task<ApiResult<List<AppointmentResponseModel>>> GetAppointmentsDoctorByUserIdInRange(Guid userId, DateTime startDay, DateTime endDate)
        {
            var repo = _unitOfWork.GetRepository<Appointment>();
            var repoChild = _unitOfWork.GetRepository<Child>();
            var repoAT = _unitOfWork.GetRepository<AppointmentTemplates>();

            // Lấy tất cả các Appointment liên quan đến userId
            var allAppointments = await repo.Entities
                .Where(x => x.AppointmentUsers.Any(au => au.DoctorId == userId) && x.DeletedBy == null && x.AppointmentDate.Date >= startDay.Date && x.AppointmentDate.Date <= endDate.Date)
                 .OrderBy(x => x.AppointmentDate)
                .ToListAsync();
            var responseList = new List<AppointmentResponseModel>();

            if (allAppointments == null || !allAppointments.Any())
            {
                return new ApiSuccessResult<List<AppointmentResponseModel>>(responseList);
            }


            // Duyệt qua từng appointment để ánh xạ dữ liệu
            foreach (var appointment in allAppointments)
            {
                var response = new AppointmentResponseModel
                {
                    Id = appointment.Id,
                    AppointmentDate = appointment.AppointmentDate,
                    Status = Enum.IsDefined(typeof(AppointmentStatus), appointment.Status)
                        ? ((AppointmentStatus)appointment.Status).ToString()
                        : "Unknown",
                    AppointmentSlot = appointment.AppointmentSlot,
                    Description = appointment.Description,
                    Fee = appointment.Fee,
                    Name = appointment.Name,
                    Result = appointment.Result,
                    Notes = appointment.Notes,
                };

                // Map User
                var user = await _userManager.FindByIdAsync(appointment.AppointmentUsers.FirstOrDefault()?.UserId.ToString());
                response.User = user != null ? _mapper.Map<UserResponseModel>(user) : null;

                // Map Doctors
                response.Doctors = new();
                foreach (var doctor in appointment.AppointmentUsers)
                {
                    if (doctor.Doctor == null) continue;

                    var doctorCheck = await _userManager.FindByIdAsync(doctor.DoctorId.ToString());
                    if (doctorCheck != null)
                    {
                        var doctorModel = _mapper.Map<EmployeeResponseModel>(doctorCheck);
                        response.Doctors.Add(doctorModel);
                    }
                }

                // Map AppointmentTemplate
                var at = await repoAT.GetByIdAsync(appointment.AppointmentTemplateId);
                response.AppointmentTemplate = at != null ? _mapper.Map<ATResponseModel>(at) : null;

                // Map Childs
                response.Childs = new();
                foreach (var child in appointment.AppointmentChildren)
                {
                    var childCheck = await repoChild.GetByIdAsync(child.ChildId);
                    if (childCheck != null)
                    {
                        var childModel = _mapper.Map<ChildModelView>(childCheck);
                        response.Childs.Add(childModel);
                    }
                }

                responseList.Add(response);
            }

            return new ApiSuccessResult<List<AppointmentResponseModel>>(responseList);
        }

        public async Task<ApiResult<AppointmentResponseModelV2>> GetAppointmentByIdSideAdmin(int id)
        {
            var repo = _unitOfWork.GetRepository<Appointment>();
            var repoChild = _unitOfWork.GetRepository<Child>();
            var repoAT = _unitOfWork.GetRepository<AppointmentTemplates>();
            var repoFGR = _unitOfWork.GetRepository<FetalGrowthRecord>();
            var repoFGSD = _unitOfWork.GetRepository<FetalGrowthStandard>();



            // Check mp is existed
            var existingItem = await repo.Entities.Include(x => x.AppointmentTemplate).Include(x => x.AppointmentUsers).ThenInclude(x => x.User)
                .Include(x => x.AppointmentChildren).ThenInclude(x => x.Child)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (existingItem == null || existingItem.DeletedBy != null)
            {
                return new ApiErrorResult<AppointmentResponseModelV2>("Appointment is not existed.");
            }
            var response = _mapper.Map<AppointmentResponseModelV2>(existingItem);


            if (Enum.IsDefined(typeof(AppointmentStatus), existingItem.Status))
            {
                response.Status = ((AppointmentStatus)existingItem.Status).ToString();
            }
            else
            {
                response.Status = "Unknown";
            }
            var user = await _userManager.FindByIdAsync(existingItem.AppointmentUsers.FirstOrDefault().UserId.ToString());

            response.User = _mapper.Map<UserResponseModel>(user);
            response.AppoinmentUsers = new();
            
            foreach (var appointmentUser in existingItem.AppointmentUsers)
            {
                if (appointmentUser.Doctor == null)
                {
                    continue;
                }
                var doctorCheck = await _userManager.FindByIdAsync(appointmentUser.DoctorId.ToString());
                if (doctorCheck != null)
                {
                    var appointmentUserModel = _mapper.Map<AppoinmentUserResponseModel>(appointmentUser);
                    appointmentUserModel.Doctor = _mapper.Map<EmployeeResponseModel>(appointmentUserModel.Doctor);

                    response.AppoinmentUsers.Add(appointmentUserModel);
                }

            }
            var at = repoAT.GetById(existingItem.AppointmentTemplateId);
            response.AppointmentTemplate = _mapper.Map<ATResponseModel>(at);
            response.Childs = new();
            foreach (var child in existingItem.AppointmentChildren)
            {
                // Map Child entity sang ChildModelViewAddeRecords
                var childModel = _mapper.Map<ChildModelViewAddeRecords>(child.Child);

                // Lấy các FGR liên quan và map cùng tiêu chuẩn
                var fgrs = await repoFGR.Entities
                    .Include(x => x.FetalGrowthStandard) // Đảm bảo include dữ liệu liên quan
                    .Where(x => x.ChildId == child.ChildId)
                    .ToListAsync();

                // Map danh sách FGR sang ModelView
                childModel.FetalGrowthRecordModelViews = _mapper.Map<List<FetalGrowthRecordModelViewAddedStandards>>(fgrs);

                // Thêm vào response
                response.Childs.Add(childModel);
            }



            return new ApiSuccessResult<AppointmentResponseModelV2>(response);
        }
        public async Task<ApiResult<object>> ChangeDoctorAppointment(ChangeDoctorAppointmentRequest request)
        {
            try
            {
                _context.Database.BeginTransaction();
                var changerId = _contextAccessor.HttpContext?.User?.FindFirst("userId");
                if (changerId == null)
                {
                    return new ApiErrorResult<object>("Plase login to use this function.", System.Net.HttpStatusCode.BadRequest);
                }
                // Tìm kiếm cuộc hẹn dựa trên AppointmentId
                var existingItem = await _context.Appointments.FirstOrDefaultAsync(x => x.Id == request.AppointmentId);

                if (existingItem == null || existingItem.DeletedBy != null)
                {
                    return new ApiErrorResult<object>("Appointment is not existed.");
                }

                // Kiểm tra xem đã có bản ghi AppointmentUser với cùng AppointmentId và DoctorId hay chưa
                var existingAppointmentUser = await _context.AppointmentUsers
                    .FirstOrDefaultAsync(x => x.AppointmentId == request.AppointmentId && x.DoctorId == request.DoctorId);

                if (existingAppointmentUser != null)
                {
                    return new ApiErrorResult<object>("This doctor is already assigned to this appointment.");
                }

                // Lấy ngày của cuộc hẹn hiện tại (loại bỏ phần giờ phút)
                var appointmentDate = existingItem.AppointmentDate.Date;

                // Kiểm tra xem bác sĩ đã có lịch trong cùng ngày và cùng slot (ngoại trừ cuộc hẹn hiện tại) hay không
                bool doctorBusy = await _context.AppointmentUsers
                    .AnyAsync(x => x.DoctorId == request.DoctorId
                        && x.AppointmentId != request.AppointmentId
                        && x.Appointment.AppointmentDate == appointmentDate
                        && x.Appointment.AppointmentSlot == existingItem.AppointmentSlot);

                if (doctorBusy)
                {
                    return new ApiErrorResult<object>("This doctor is busy at the selected time slot.");
                }

                var userId = existingItem.AppointmentUsers.FirstOrDefault()?.UserId ?? Guid.Empty;
                if (userId == Guid.Empty)
                {
                    return new ApiErrorResult<object>("User is not existed.");
                }

                var auId = await GetNextAppointmentUserIdAsync();
                var appointmentUser = new AppointmentUser()
                {
                    //Id = auId,
                    AppointmentId = request.AppointmentId,
                    DoctorId = request.DoctorId,
                    AssignedTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    Description = existingItem.Description,
                    UserId = userId,
                    LastUpdatedTime = DateTime.Now,
                    Reason = request.Reason,
                    AssignedBy = Guid.Parse(changerId.Value),
                };

                _context.AppointmentUsers.Add(appointmentUser);
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
                return new ApiSuccessResult<object>("Doctor appointment changed successfully.");
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();
                throw new Exception("Transaction failed.", ex);
            }
        }


    }
}
