
namespace BabyCare.ModelViews.ChildModelView
{
    public class ChildModelView
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string FetalGender { get; set; }
        public DateTime DueDate { get; set; }
        public string? PhotoUrl { get; set; }
        public string? BloodType { get; set; }

    }
}
