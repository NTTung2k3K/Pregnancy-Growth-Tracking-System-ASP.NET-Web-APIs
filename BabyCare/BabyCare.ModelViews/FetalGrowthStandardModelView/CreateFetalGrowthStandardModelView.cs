namespace BabyCare.ModelViews.FetalGrowthStandardModelView
{
    public class CreateFetalGrowthStandardModelView
    {
        public int Week { get; set; }
        public int Gender { get; set; }

        public string? GestationalAge { get; set; }
        public float MinWeight { get; set; }
        public float MaxWeight { get; set; }
        public float AverageWeight { get; set; }
        public float MinHeight { get; set; }
        public float MaxHeight { get; set; }
        public float AverageHeight { get; set; }
        public float HeadCircumference { get; set; }
        public float AbdominalCircumference { get; set; }
        public int? FetalHeartRate { get; set; }
    }
}
