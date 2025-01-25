﻿namespace BabyCare.ModelViews.FeedbackModelView
{
    using Microsoft.AspNetCore.Http;
    using System.ComponentModel.DataAnnotations;

    public class CreateFeedbackModelView
    {
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "GrowthChart ID is required.")]
        public int GrowthChartsID { get; set; }

        public int? ParentFeedbackID { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        public string? FeedbackType { get; set; }

        public string? Status { get; set; }

        public bool IsAnonymous { get; set; }
    }
}
