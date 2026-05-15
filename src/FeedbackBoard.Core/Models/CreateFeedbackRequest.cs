using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackBoard.Core.Models
{
    public class CreateFeedbackRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
    }
}
