using FeedbackBoard.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackBoard.Core.Models
{
    public class ChangeStatusRequest
    {
        public FeedbackStatusEnum NewStatus { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
}
