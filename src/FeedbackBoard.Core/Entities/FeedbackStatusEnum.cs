using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackBoard.Core.Entities
{
    public enum FeedbackStatusEnum
    {
        New = 1,
        UnderReview = 2,
        Planned = 3,
        InProgress = 4,
        Completed = 5,
        Declined = 6,
        Duplicate = 7
    }
}
