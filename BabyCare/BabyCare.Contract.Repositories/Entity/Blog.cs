using BabyCare.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Repositories.Entity
{
    public class Blog : BaseEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int? Week { get; set; }
        [ForeignKey("AuthorId")]
        public Guid AuthorId { get; set; }


        public int LikesCount { get; set; } = 0;
        public int ViewCount { get; set; } = 0;
        public int? Status { get; set; }
        public string? Sources { get; set; }
        public string Thumbnail { get; set; }
        [ForeignKey("BlogTypeId")]
        public int BlogTypeId { get; set; }


        //public bool IsFeatured { get; set; } = false;

        public virtual ApplicationUsers Author { get; set; }
        public virtual BlogType BlogType { get; set; }
    }

}
