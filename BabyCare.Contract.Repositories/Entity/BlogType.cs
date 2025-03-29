using BabyCare.Core.Base;

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
