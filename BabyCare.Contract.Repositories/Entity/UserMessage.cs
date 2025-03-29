using BabyCare.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Repositories.Entity
{
    public class UserMessage : BaseEntity
    {
        [ForeignKey("UserId")]
        public Guid UserId { get; set; }
        [ForeignKey("RecipientUserId")]
        public Guid RecipientUserId { get; set; }
        public virtual ApplicationUsers RecipientUser { get; set; }
        public virtual ApplicationUsers User { get; set; }

        public DateTime SendAt { get; set; }
        public string MessageContent { get; set; }
        public string ChannelName { get; set; }
    }
}
