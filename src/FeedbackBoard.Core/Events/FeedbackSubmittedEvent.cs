using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackBoard.Core.Events
{
    public class FeedbackSubmittedEvent
    {
        public string FeedbackId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
