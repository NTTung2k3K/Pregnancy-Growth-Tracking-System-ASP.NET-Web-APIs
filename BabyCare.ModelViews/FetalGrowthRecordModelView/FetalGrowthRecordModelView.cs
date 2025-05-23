﻿namespace BabyCare.ModelViews.FetalGrowthRecordModelView
{
    public class FetalGrowthRecordModelView
    {
        public int Id { get; set; }
        public int ChildId { get; set; }
        public int WeekOfPregnancy { get; set; }
        public float Weight { get; set; }
        public float Height { get; set; }
        public DateTime RecordedAt { get; set; }
        public string? HealthCondition { get; set; }
        public float? HeadCircumference { get; set; }
        public float? AbdominalCircumference { get; set; }
        public int? FetalHeartRate { get; set; }

    }
}
