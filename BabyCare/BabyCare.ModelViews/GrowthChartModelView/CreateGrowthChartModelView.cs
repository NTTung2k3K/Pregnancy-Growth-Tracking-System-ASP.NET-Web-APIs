﻿namespace BabyCare.ModelViews.GrowthChartModelView
{
    public class CreateGrowthChartModelView
    {

        public int ChildId { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
        public string Topic { get; set; }
        public string? Question { get; set; }
    }
}
