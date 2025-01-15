using BabyCare.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using TKFamilyFinance.API.Entities;

namespace BabyCare.Data.Context
{
    public class BabyCareContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public BabyCareContext(DbContextOptions<BabyCareContext> options) : base(options) { }
    }
}
