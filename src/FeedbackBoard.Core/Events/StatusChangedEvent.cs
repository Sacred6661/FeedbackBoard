using FeedbackBoard.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackBoard.Core.Events
{
    public class StatusChangedEvent
    {
        public required string FeedbackId { get; set; }
        public FeedbackStatusEnum OldStatus { get; set; }
        public FeedbackStatusEnum NewStatus { get; set; }
        public required string ChangedBy { get; set; }
        public string? Reason { get; set; }
    }
}
