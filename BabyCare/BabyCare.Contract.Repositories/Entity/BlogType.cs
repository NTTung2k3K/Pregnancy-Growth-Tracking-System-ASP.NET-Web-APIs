using BabyCare.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Repositories.Entity
{
    public class BlogType : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Thumbnail { get; set; }

        public virtual ICollection<Blog> Blogs { get; set; }
    }

}
