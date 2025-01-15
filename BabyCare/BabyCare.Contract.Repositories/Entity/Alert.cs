using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabyCare.Core.Base;

namespace BabyCare.Contract.Repositories.Entity
{
    public class Alert : BaseEntity
    {
        public int RecordId { get; set; }

        [ForeignKey("RecordId")]

        public string Title { get; set; }
        public string? Description { get; set; }
        public bool IsRead { get; set; } = false;
        public string? Type { get; set; }
        public DateTime DateAlerted { get; set; }

        public virtual FetalGrowthRecord Record { get; set; }
    }

}
