using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackBoard.Core.Models
{
    public class StatusChangeResponse
    {
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
