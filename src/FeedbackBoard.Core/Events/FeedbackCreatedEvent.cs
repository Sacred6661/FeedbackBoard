using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackBoard.Core.Events
{
    public class FeedbackCreatedEvent
    {
        public required string FeedbackId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
