using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackBoard.Core.Events
{
    public class FeedbackVotedEvent
    {
        public required string FeedbackId { get; set; }
        public required string UserId { get; set; }
        public int NewVoteCount { get; set; }
    }
}
