using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.IUOW;
using BabyCare.Repositories.Context;
using Microsoft.EntityFrameworkCore;

namespace BabyCare.Repositories.UOW
{
    public class RoleRepository : GenericRepository<ApplicationRoles>, IRoleRepository
    {
        private readonly DatabaseContext _dbContext;

		protected readonly DbSet<ApplicationRoles> _dbSet;

		public RoleRepository(DatabaseContext context) : base(context)
        {
            _dbContext = context;

			_dbSet = _context.Set<ApplicationRoles>();
		}
    }
}
