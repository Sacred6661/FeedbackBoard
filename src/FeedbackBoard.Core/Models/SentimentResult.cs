using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackBoard.Core.Models
{
    public class SentimentResult
    {
        public string Sentiment { get; set; } = "Neutral";
        public double Confidence { get; set; }
    }
}
