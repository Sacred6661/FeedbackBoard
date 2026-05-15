using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackBoard.Core.Entities
{
    public class StatusChange
    {
        public FeedbackStatusEnum OldStatus { get; set; }
        public FeedbackStatusEnum NewStatus { get; set; }
        public string ChangedBy { get; set; } = "system";
        public string? Reason { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
