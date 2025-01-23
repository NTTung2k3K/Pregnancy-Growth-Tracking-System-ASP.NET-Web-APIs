using AutoMapper;
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
using BabyCare.ModelViews.UserModelViews.Response;
using BabyCare.Repositories.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.Services.Service
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUsers> _userManager;
        private readonly DatabaseContext _context;

        public AppointmentService(DatabaseContext context, IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor, UserManager<ApplicationUsers> userManager)
        {
            _context = context;
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
            var existingChild = await repoChild.GetByIdAsync(request.ChildId);
            if (existingChild == null)
            {
                return new ApiErrorResult<object>("Child is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Check appointment template is existed
            var existingAT = await repoAT.GetByIdAsync(request.AppointmentTemplateId);
            if (existingAT == null)
            {
                return new ApiErrorResult<object>("Appointment Type is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Check available slot
            var slotOnDay = repoAppointment.Entities.Where(x => x.AppointmentDate.Date == request.AppointmentDate.Date);
            if (slotOnDay.Count() == SystemConstant.MAX_SLOT_AVAILABLE_APPOINTMENT)
            {
                return new ApiErrorResult<object>("Day " + request.AppointmentDate.Date.ToShortDateString() + " is full of slot. Please choose another day!", System.Net.HttpStatusCode.NotFound);
            }
            if (slotOnDay.Any(x => x.AppointmentSlot == request.AppointmentSlot))
            {
                return new ApiErrorResult<object>(
                    "Slot has been selected by other. Please choose another slot",
                    System.Net.HttpStatusCode.NotFound
                );
            }

            //try
            //{
            //    // Bắt đầu giao dịch từ _context
            //    using (var transaction = await _context.Database.BeginTransactionAsync())
            //    {
            //        // Tạo Appointment mới
            //        var appointment = new Appointment()
            //        {
            //            AppointmentTemplateId = request.AppointmentTemplateId,
            //            AppointmentSlot = request.AppointmentSlot,
            //            AppointmentDate = request.AppointmentDate,
            //            Status = (int)AppointmentStatus.Confirmed,
            //            Fee = existingAT.Fee,
            //            Notes = request.Notes,
            //            Name = request.Name,
            //        };

            //        // Thêm Appointment vào DbContext và lưu vào database
            //        await _context.Appointments.AddAsync(appointment);
            //        await _context.SaveChangesAsync();  // Lưu Appointment vào database

            //        // Kiểm tra lại xem Appointment.Id đã có giá trị hợp lệ
            //        if (appointment.Id <= 0)
            //        {
            //            throw new Exception("Appointment was not saved correctly, Id is invalid.");
            //        }

            //        // Lấy DoctorId từ hàm GetRandomDoctorIdAsync
            //        var doctorId = await GetRandomDoctorIdAsync();

            //        // Lấy AppointmentUserId tiếp theo
            //        var auId = await GetNextAppointmentUserIdAsync();

            //        // Tạo AppointmentUser mới
            //        var appointmentUser = new AppointmentUser()
            //        {
            //            Appointment = appointment,
            //            AppointmentId = appointment.Id,
            //            CreatedTime = DateTime.Now,
            //            Description = $"{existingAT.Name}_{appointment.AppointmentDate.ToString()}_Slot:{appointment.AppointmentSlot}_Estimate Cost:{appointment.Fee}",
            //            UserId = request.UserId,
            //            DoctorId = doctorId != null ? doctorId.Value : null,
            //        };

            //        // Thêm AppointmentUser vào DbContext và lưu vào database
            //        await _context.AppointmentUsers.AddAsync(appointmentUser);
            //        await _context.SaveChangesAsync();  // Lưu AppointmentUser vào database

            //        // Tạo AppointmentChild mới
            //        var appointmentChild = new AppointmentChild()
            //        {
            //            Appointment = appointment,
            //            Child = existingChild,
            //            Description = $"{existingChild.Name}_{request.Name}",
            //        };

            //        // Thêm AppointmentChild vào DbContext và lưu vào database
            //        await _context.AppointmentChildren.AddAsync(appointmentChild);
            //        await _context.SaveChangesAsync();  // Lưu AppointmentChild vào database

            //        // Commit giao dịch sau khi tất cả các thao tác đã hoàn thành
            //        await transaction.CommitAsync();

            //        return new ApiSuccessResult<object>("Create successfully.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    // Rollback giao dịch nếu có lỗi
            //    //await transaction.RollbackAsync();
            //    throw new Exception("Transaction failed.", ex);
            //}


            try
            {
                _unitOfWork.BeginTransaction();
                var appointment = new Appointment()
                {
                    AppointmentTemplateId = request.AppointmentTemplateId,
                    AppointmentSlot = request.AppointmentSlot,
                    AppointmentDate = request.AppointmentDate,
                    Status = (int)AppointmentStatus.Confirmed,
                    Fee = existingAT.Fee,
                    Notes = request.Notes,
                    Name = request.Name,
                };
                await repoAppointment.InsertAsync(appointment);
                await repoAppointment.SaveAsync();
                await _unitOfWork.SaveAsync();
                var doctorId = await GetRandomDoctorIdAsync();
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
                };
                await repoAppointmentUser.InsertAsync(appointmentUser);
                await repoAppointmentUser.SaveAsync();

                var appointmentChild = new AppointmentChild()
                {
                    Appointment = appointment,
                    Child = existingChild,
                    Description = $"{existingChild.Name}_{request.Name}",
                };
                await repoAC.InsertAsync(appointmentChild);
                await repoAC.SaveAsync();


                _unitOfWork.CommitTransaction();
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
            existingItem.DeletedTime = DateTime.Now;
            existingItem.DeletedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

            await repo.UpdateAsync(existingItem);
            await repo.SaveAsync();

            return new ApiSuccessResult<object>("Delete successfully.");
        }

        public async Task<ApiResult<AppointmentResponseModel>> GetAppointmentById(int id)
        {
            var repo = _unitOfWork.GetRepository<Appointment>();
            var repoChild = _unitOfWork.GetRepository<Child>();
            var repoAT = _unitOfWork.GetRepository<AppointmentTemplates>();

            // Check mp is existed
            var existingItem = await repo.Entities.Include(x => x.AppointmentTemplate).Include(x => x.AppointmentUsers).ThenInclude(x => x.User)
                .Include(x => x.AppointmentChildren).ThenInclude(x => x.Child)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (existingItem == null || existingItem.DeletedBy != null)
            {
                return new ApiErrorResult<AppointmentResponseModel>("Appointment is not existed.");
            }
            var response = _mapper.Map<AppointmentResponseModel>(existingItem);


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
            response.Doctors = new();
            foreach (var doctor in existingItem.AppointmentUsers)
            {
                if (doctor.Doctor == null)
                {
                    continue;
                }
                var doctorCheck = await _userManager.FindByIdAsync(doctor.DoctorId.ToString());
                var doctorModel = _mapper.Map<EmployeeResponseModel>(doctorCheck);
                response.Doctors.Add(doctorModel);
            }
            var at = repoAT.GetById(existingItem.AppointmentTemplateId);
            response.AppointmentTemplate = _mapper.Map<ATResponseModel>(at);
            response.Childs = new();
            foreach (var child in existingItem.AppointmentChildren)
            {
                var childCheck = repoChild.GetById(child.ChildId);
                var childModel = _mapper.Map<ChildModelView>(childCheck);
                response.Childs.Add(childModel);
            }


            return new ApiSuccessResult<AppointmentResponseModel>(response);
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
            int MAX_SLOT_AVAILABLE_APPOINTMENT = SystemConstant.MAX_SLOT_AVAILABLE_APPOINTMENT;
            var repo = _unitOfWork.GetRepository<Appointment>();

            var slotsBookedOnDay = await repo.Entities
                .Where(x => x.AppointmentDate == date)
                .Select(x => x.AppointmentSlot)
                .ToListAsync();

            var allSlots = Enumerable.Range(1, MAX_SLOT_AVAILABLE_APPOINTMENT).ToList();

            var availableSlots = allSlots.Except(slotsBookedOnDay).ToList();

            var response = new AvailableSlotResponseModel
            {
                Date = date,
                Slots = availableSlots
            };

            return new ApiSuccessResult<AvailableSlotResponseModel>(response);
        }

        public async Task<ApiResult<List<AppointmentResponseModel>>> GetAppointmentsByUserId(Guid userId)
        {
            var repo = _unitOfWork.GetRepository<Appointment>();
            var repoChild = _unitOfWork.GetRepository<Child>();
            var repoAT = _unitOfWork.GetRepository<AppointmentTemplates>();

            // Lấy tất cả các Appointment liên quan đến userId
            var allAppointments = await repo.Entities
                .Include(x => x.AppointmentTemplate)
                .Include(x => x.AppointmentUsers).ThenInclude(x => x.User)
                .Include(x => x.AppointmentChildren).ThenInclude(x => x.Child)
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
                .Include(x => x.AppointmentTemplate)
                .Include(x => x.AppointmentUsers).ThenInclude(x => x.User)
                .Include(x => x.AppointmentChildren).ThenInclude(x => x.Child)
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
    }
}
