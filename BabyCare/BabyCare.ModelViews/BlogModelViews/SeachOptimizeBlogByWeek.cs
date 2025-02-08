﻿using BabyCare.ModelViews.AppointmentModelViews.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.BlogModelViews
{
    public class SeachOptimizeBlogByWeek : SearchOptimizeRequest
    {
        public int? BlogTypeId { get; set; }
        public int? Week { get; set; }
    }
}
