﻿using AutoMapper;
using Azure.Core;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.ChildModelView;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.Services.Service
{
    public class ChildService : IChildService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public ChildService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
        }

        public async Task<ApiResult<object>> AddChildAsync(CreateChildModelView model)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                // Kiểm tra nếu một đứa trẻ với tên và ngày sinh đã tồn tại
                var existingChild = await _unitOfWork.GetRepository<Child>()
                    .Entities
                    .FirstOrDefaultAsync(c => c.Name.Equals(model.Name) && c.DueDate == model.DueDate && !c.DeletedTime.HasValue);

                if (existingChild != null)
                {
                    return new ApiErrorResult<object>("A child with the same name and date of birth already exists.");
                }

                // Ánh xạ từ CreateChildModelView sang Child entity
                Child newChild = _mapper.Map<Child>(model);
                newChild.IsGenerateSampleAppointments = model.IsGenerateSampleAppointments;

                // Upload ảnh nếu có
                if (model.PhotoUrl != null)
                {
                    newChild.PhotoUrl = await BabyCare.Core.Firebase.ImageHelper.Upload(model.PhotoUrl);
                }

                // Thiết lập các trường còn lại
                newChild.CreatedTime = DateTimeOffset.UtcNow;
                newChild.CreatedBy = model.UserId.ToString(); // Hoặc lấy từ hệ thống User nếu có


                // Lưu thông tin đứa trẻ vào cơ sở dữ liệu
                await _unitOfWork.GetRepository<Child>().InsertAsync(newChild);
                await _unitOfWork.SaveAsync();


                // Generate appointment
                if (newChild.IsGenerateSampleAppointments)
                {
                    await GenerateAppointment(model.UserId, newChild, model.DueDate);

                }
                _unitOfWork.CommitTransaction();

                return new ApiSuccessResult<object>("Child added successfully.");
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new Exception(ex.Message);
            }

        }

        private async Task GenerateAppointment(Guid userId, Child existingChild, DateTime dueDate)
        {
            var appointmentRepo = _unitOfWork.GetRepository<Appointment>();
            var appointmentUserRepo = _unitOfWork.GetRepository<AppointmentUser>();
            var appointmentChildRepo = _unitOfWork.GetRepository<AppointmentChild>();



            var appointmentTemplatesRepo = _unitOfWork.GetRepository<AppointmentTemplates>();
            var allAT = appointmentTemplatesRepo.GetAll();
            foreach (var appointmentTemplates in allAT)
            {
                var appointment = new Appointment()
                {
                    AppointmentTemplateId = appointmentTemplates.Id,
                    Status = (int)AppointmentStatus.Pending,
                    AppointmentDate = dueDate.AddDays(appointmentTemplates.DaysFromBirth),
                    Fee = appointmentTemplates.Fee,
                    Name = appointmentTemplates.Name,
                };
                await appointmentRepo.InsertAsync(appointment);
                await appointmentRepo.SaveAsync();
                await _unitOfWork.SaveAsync();
                //var doctorId = await GetRandomDoctorIdAsync();
                //var auId = await GetNextAppointmentUserIdAsync();
                var appointmentUser = new AppointmentUser()
                {
                    //Id = auId,
                    AppointmentId = appointment.Id,
                    Appointment = appointment,
                    UserId = userId,
                    //DoctorId = doctorId != null ? doctorId.Value : null,
                };
                await appointmentUserRepo.InsertAsync(appointmentUser);
                await appointmentUserRepo.SaveAsync();

                var appointmentChild = new AppointmentChild()
                {
                    Appointment = appointment,
                    Child = existingChild,
                    //Description = $"{existingChild.Name}_{request.Name}",
                };
                await appointmentChildRepo.InsertAsync(appointmentChild);
                await appointmentChildRepo.SaveAsync();
            }


        }

        public async Task<ApiResult<object>> DeleteChildAsync(int id)
        {
            // Kiểm tra sự tồn tại của đứa trẻ
            var child = await _unitOfWork.GetRepository<Child>()
                .Entities
                .FirstOrDefaultAsync(c => c.Id == id && !c.DeletedTime.HasValue);

            // Nếu không tìm thấy đứa trẻ, trả về lỗi
            if (child == null)
            {
                return new ApiErrorResult<object>("Child not found or already deleted.");
            }

            // Đánh dấu thời gian xóa (soft delete)
            child.DeletedTime = DateTimeOffset.UtcNow;

            // Cập nhật lại thông tin xóa của đứa trẻ
            child.DeletedBy = "System"; // Hoặc có thể là userId của người xóa nếu có thông tin

            // Cập nhật đứa trẻ trong cơ sở dữ liệu
            await _unitOfWork.GetRepository<Child>().UpdateAsync(child);
            await _unitOfWork.SaveAsync();

            // Trả về thông báo thành công
            return new ApiSuccessResult<object>("Child successfully deleted.");
        }

        public async Task<ApiResult<BasePaginatedList<ChildModelView>>> GetAllChildAsync(int pageNumber, int pageSize, int? id, string? name, DateTime? dueDate, string? bloodType, string? pregnancyStage)
        {
            // Khởi tạo query cơ bản cho bảng Child
            IQueryable<Child> childQuery = _unitOfWork.GetRepository<Child>().Entities
                .AsNoTracking()
                .Where(c => !c.DeletedTime.HasValue); // Loại bỏ các bản ghi đã bị xóa

            // Áp dụng bộ lọc theo id, name, dateOfBirth, bloodType, và pregnancyStage nếu có
            if (id != null)
                childQuery = childQuery.Where(c => c.Id == id);

            if (!string.IsNullOrWhiteSpace(name))
                childQuery = childQuery.Where(c => c.Name.Contains(name));

            if (dueDate.HasValue)
                childQuery = childQuery.Where(c => c.DueDate == dueDate.Value);

            if (!string.IsNullOrWhiteSpace(bloodType))
                childQuery = childQuery.Where(c => c.BloodType.Contains(bloodType));

            if (!string.IsNullOrWhiteSpace(pregnancyStage))
                childQuery = childQuery.Where(c => c.PregnancyStage.Contains(pregnancyStage));

            // Sắp xếp theo thời gian tạo giảm dần
            childQuery = childQuery.OrderByDescending(c => c.CreatedTime);

            // Lấy tổng số lượng bản ghi
            int totalCount = await childQuery.CountAsync();

            // Lấy dữ liệu phân trang
            List<Child> paginatedChildren = await childQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Chuyển đổi từ Child sang ChildModelView
            List<ChildModelView> childModelViews = _mapper.Map<List<ChildModelView>>(paginatedChildren);

            // Tạo đối tượng phân trang
            var result = new BasePaginatedList<ChildModelView>(childModelViews, totalCount, pageNumber, pageSize);

            // Trả về kết quả
            return new ApiSuccessResult<BasePaginatedList<ChildModelView>>(result);
        }

        public async Task<ApiResult<ChildModelView>> GetChildByIdAsync(int id)
        {
            if (id <= 0)
            {
                return new ApiErrorResult<ChildModelView>("Please provide a valid Child ID.");
            }

            // Lấy Child từ cơ sở dữ liệu
            var childEntity = await _unitOfWork.GetRepository<Child>().Entities
                .AsNoTracking()
                .FirstOrDefaultAsync(child => child.Id == id && !child.DeletedTime.HasValue);

            if (childEntity == null)
            {
                return new ApiErrorResult<ChildModelView>("Child not found or has been deleted.");
            }

            // Chuyển đổi từ Child sang ChildModelView
            ChildModelView childModelView = _mapper.Map<ChildModelView>(childEntity);

            return new ApiSuccessResult<ChildModelView>(childModelView);
        }

        public async Task<ApiResult<object>> UpdateChildAsync(int id, UpdateChildModelView model)
        {
            // Kiểm tra ID hợp lệ
            if (id <= 0)
            {
                return new ApiErrorResult<object>("Please provide a valid Child ID.");
            }

            // Tìm child theo ID và kiểm tra xem nó có tồn tại không
            var existingChild = await _unitOfWork.GetRepository<Child>()
                .Entities
                .FirstOrDefaultAsync(child => child.Id == id && !child.DeletedTime.HasValue);

            if (existingChild == null)
            {
                return new ApiErrorResult<object>("Child not found or has been deleted.");
            }

            // Cập nhật các trường nếu có sự thay đổi
            bool isUpdated = false;

            if (!string.IsNullOrWhiteSpace(model.Name) && model.Name != existingChild.Name)
            {
                existingChild.Name = model.Name;
                isUpdated = true;
            }

            if (model.DueDate.HasValue && model.DueDate != existingChild.DueDate)
            {
                existingChild.DueDate = model.DueDate.Value;
                isUpdated = true;
            }

            if (model.DueDate.HasValue && model.DueDate != existingChild.DueDate)
            {
                existingChild.DueDate = model.DueDate.Value;
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(model.FetalGender) && model.FetalGender != existingChild.FetalGender)
            {
                existingChild.FetalGender = model.FetalGender;
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(model.PregnancyStage) && model.PregnancyStage != existingChild.PregnancyStage)
            {
                existingChild.PregnancyStage = model.PregnancyStage;
                isUpdated = true;
            }

            if (model.WeightEstimate.HasValue && model.WeightEstimate != existingChild.WeightEstimate)
            {
                existingChild.WeightEstimate = model.WeightEstimate.Value;
                isUpdated = true;
            }

            if (model.HeightEstimate.HasValue && model.HeightEstimate != existingChild.HeightEstimate)
            {
                existingChild.HeightEstimate = model.HeightEstimate.Value;
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(model.DeliveryPlan) && model.DeliveryPlan != existingChild.DeliveryPlan)
            {
                existingChild.DeliveryPlan = model.DeliveryPlan;
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(model.Complications) && model.Complications != existingChild.Complications)
            {
                existingChild.Complications = model.Complications;
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(model.BloodType) && model.BloodType != existingChild.BloodType)
            {
                existingChild.BloodType = model.BloodType;
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(model.PregnancyWeekAtBirth) && model.PregnancyWeekAtBirth != existingChild.PregnancyWeekAtBirth)
            {
                existingChild.PregnancyWeekAtBirth = model.PregnancyWeekAtBirth;
                isUpdated = true;
            }

            // Upload photo nếu có
            if (model.PhotoUrl != null)
            {
                existingChild.PhotoUrl = await BabyCare.Core.Firebase.ImageHelper.Upload(model.PhotoUrl);
                isUpdated = true;
            }

            try
            {
                _unitOfWork.BeginTransaction();
                // Nếu có thay đổi, cập nhật thông tin và lưu vào DB
                if (isUpdated)
                {
                    existingChild.LastUpdatedTime = DateTimeOffset.UtcNow;
                    // Bạn có thể sử dụng thông tin userId từ context nếu cần
                    // existingChild.LastUpdatedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

                    await _unitOfWork.GetRepository<Child>().UpdateAsync(existingChild);
                    await _unitOfWork.SaveAsync();
                }
                //Generate appointment
                if (model.UserId == null)
                {
                    return new ApiErrorResult<object>("User is not existed.");
                }
                if (model.IsGenerateSampleAppointments == true)
                {
                    if (existingChild.IsGenerateSampleAppointments == false)
                    {
                        await GenerateAppointment(model.UserId.Value, existingChild, existingChild.DueDate);
                    }
                    else
                    {
                        return new ApiErrorResult<object>("Appointment has been generated.");

                    }
                }
                _unitOfWork.CommitTransaction();

                return new ApiSuccessResult<object>("Child updated successfully.");
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new Exception(ex.Message);
            }

        }

        public async Task<ApiResult<List<ChildModelView>>> GetChildByUserId(Guid id)
        {
            // Lấy Child từ cơ sở dữ liệu
            var childEntity =  _unitOfWork.GetRepository<Child>().Entities
                .AsNoTracking()
                .Where(child => child.UserId == id && !child.DeletedTime.HasValue);

        

            // Chuyển đổi từ Child sang ChildModelView
            var childModelView = _mapper.Map<List<ChildModelView>>(childEntity);

            return new ApiSuccessResult<List<ChildModelView>>(childModelView);
        }
    }
}
