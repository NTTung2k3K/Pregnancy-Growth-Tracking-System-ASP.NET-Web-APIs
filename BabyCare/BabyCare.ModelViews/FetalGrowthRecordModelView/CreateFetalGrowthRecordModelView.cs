﻿namespace BabyCare.ModelViews.FetalGrowthRecordModelView
{
    public class CreateFetalGrowthRecordModelView
    {
        public int ChildId { get; set; }
        public int WeekOfPregnancy { get; set; }
        public float Weight { get; set; }
        public float Height { get; set; }
        public DateTime RecordedAt { get; set; }
        public int? GrowChartsID { get; set; }
        public string? HealthCondition { get; set; }
    }
}
