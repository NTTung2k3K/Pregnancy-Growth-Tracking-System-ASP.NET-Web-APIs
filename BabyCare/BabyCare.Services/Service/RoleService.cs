using AutoMapper;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.RoleModelViews;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BabyCare.Services.Service
{
	public class RoleService : IRoleService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _contextAccessor;

		public RoleService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_contextAccessor = contextAccessor;
		}

		public async Task<ApiResult<BasePaginatedList<RoleModelView>>> GetAllRoleAsync(int pageNumber, int pageSize, string? id, string? name)
		{
			IQueryable<ApplicationRoles> roleQuery = _unitOfWork.GetRepository<ApplicationRoles>().Entities
				.AsNoTracking()
				.Where(p => !p.DeletedTime.HasValue);

			if (!string.IsNullOrWhiteSpace(id))
				roleQuery = roleQuery.Where(p => p.Id.ToString() == id);

			if (!string.IsNullOrWhiteSpace(name))
				roleQuery = roleQuery.Where(p => p.Name.Contains(name));

			roleQuery = roleQuery.OrderByDescending(r => r.CreatedTime);

			int totalCount = await roleQuery.CountAsync();

			List<ApplicationRoles> paginatedRoles = await roleQuery
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			List<RoleModelView> roleModelViews = _mapper.Map<List<RoleModelView>>(paginatedRoles);
			var result = new BasePaginatedList<RoleModelView>(roleModelViews, totalCount, pageNumber, pageSize);

			return new ApiSuccessResult<BasePaginatedList<RoleModelView>>(result);
		}

		public async Task<ApiResult<object>> AddRoleAsync(CreateRoleModelView model)
		{
			var existedRole = await _unitOfWork.GetRepository<ApplicationRoles>()
				.Entities
				.FirstOrDefaultAsync(role => role.Name.Equals(model.Name) && !role.DeletedTime.HasValue);

			if (existedRole != null)
			{
				return new ApiErrorResult<object>("Role already exists");
			}

			ApplicationRoles newRole = _mapper.Map<ApplicationRoles>(model);


			/*if (userId != null)
			{
				newRole.CreatedBy = userId;
			}
			else
			{
				newRole.CreatedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
			}*/

			newRole.CreatedBy = model.Name;
			newRole.CreatedTime = DateTimeOffset.UtcNow;

			await _unitOfWork.GetRepository<ApplicationRoles>().InsertAsync(newRole);

			await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Role added successfully");

		}

		public async Task<ApiResult<object>> UpdateRoleAsync(string id, UpdatedRoleModelView model)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
                return new ApiErrorResult<object>("Please provide a valid Role ID.");
			}

			var existingRole = await _unitOfWork.GetRepository<ApplicationRoles>().Entities
				.FirstOrDefaultAsync(s => s.Id == Guid.Parse(id) && !s.DeletedTime.HasValue);

			if (existingRole == null)
			{
                return new ApiErrorResult<object>("The Role cannot be found or has been deleted!");

			}

			bool isUpdated = false;

			if (!string.IsNullOrWhiteSpace(model.Name) && model.Name != existingRole.Name)
			{
				var roleWithSameName = await _unitOfWork.GetRepository<ApplicationRoles>().Entities
					.AnyAsync(s => s.Name == model.Name && !s.DeletedTime.HasValue);

				if (roleWithSameName)
				{
                    return new ApiErrorResult<object>("A role with the same name already exists.");

				}

				existingRole.Name = model.Name;
				isUpdated = true;
			}

			if (isUpdated)
			{
				/*if (userId != null)
				{
					existingRole.LastUpdatedBy = userId;
				}
				else
				{
					existingRole.LastUpdatedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
				}*/
				existingRole.LastUpdatedBy = model.Name;
				existingRole.LastUpdatedTime = DateTimeOffset.UtcNow;

				await _unitOfWork.GetRepository<ApplicationRoles>().UpdateAsync(existingRole);
				await _unitOfWork.SaveAsync();
			}
            return new ApiErrorResult<object>("Role successfully updated.");

		}

		public async Task<ApiResult<object>> DeleteRoleAsync(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
                return new ApiErrorResult<object>("Please provide a valid Role ID.");

			}

			var existingRole = await _unitOfWork.GetRepository<ApplicationRoles>().Entities
				.FirstOrDefaultAsync(s => s.Id == Guid.Parse(id) && !s.DeletedTime.HasValue);

			if (existingRole == null)
			{
                return new ApiErrorResult<object>("The Role cannot be found or has been deleted!");

			}

			existingRole.DeletedTime = DateTimeOffset.UtcNow;

			/*if (userId != null)
			{
				existingRole.DeletedBy = userId;
			}
			else
			{
				existingRole.DeletedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
			}*/
			existingRole.DeletedBy = existingRole.Name;

			await _unitOfWork.GetRepository<ApplicationRoles>().UpdateAsync(existingRole);
			await _unitOfWork.SaveAsync();
            return new ApiErrorResult<object>("Role successfully deleted.");

		}

		public async Task<ApiResult<RoleModelView>> GetRoleByIdAsync(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
                return new ApiErrorResult<RoleModelView>("Please provide a valid Role ID.");

            }

			var roleEntity = await _unitOfWork.GetRepository<ApplicationRoles>().Entities
				.FirstOrDefaultAsync(role => role.Id == Guid.Parse(id) && !role.DeletedTime.HasValue);

			if (roleEntity == null)
			{
                return new ApiErrorResult<RoleModelView>("Role is not exited.");

			}

			RoleModelView roleModelView = _mapper.Map<RoleModelView>(roleEntity);
            return new ApiSuccessResult<RoleModelView>(roleModelView);

		}
	}
}