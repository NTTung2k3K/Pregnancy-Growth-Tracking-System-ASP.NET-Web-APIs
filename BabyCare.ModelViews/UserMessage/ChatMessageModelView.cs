using BabyCare.ModelViews.UserModelViews.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.UserMessage
{
    public class ChatMessageModelView
    {
        public int Id { get; set; }
        public EmployeeResponseModel SenderId { get; set; }
        public EmployeeResponseModel ReceiverId { get; set; }
        public string Message { get; set; }
    }
}
