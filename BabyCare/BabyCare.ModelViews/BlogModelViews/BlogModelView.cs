using BabyCare.ModelViews.BlogTypeModelView;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.BlogModelViews
{
    public class BlogModelView
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public Guid AuthorId { get; set; }
        public int LikesCount { get; set; }
        public int? Week { get; set; }

        public int ViewCount { get; set; }
        public string Status { get; set; }
        public string Sources { get; set; }
        public string Thumbnail { get; set; }
        public int BlogTypeId { get; set; }
        public bool IsFeatured { get; set; }
    }
    public class BlogModelViewAddedType : BlogModelView
    {
        public BlogTypeModelView.BlogTypeModelView BlogTypeModelView { get; set; }
    } 
}
