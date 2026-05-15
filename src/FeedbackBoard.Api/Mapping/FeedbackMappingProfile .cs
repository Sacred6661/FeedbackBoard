using FeedbackBoard.Core.Entities;
using FeedbackBoard.Core.Models;
using Mapster;

namespace FeedbackBoard.Api.Mapping;

public class FeedbackMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Feedback, FeedbackResponse>()
            .Map(dest => dest.StatusInfo, src => src)
            .Map(dest => dest.Sentiment, src => src.AiAnalysis != null ? src.AiAnalysis.Sentiment : null)
            .Map(dest => dest.HasDuplicates, src => src.AiAnalysis != null && src.AiAnalysis.HasDuplicates)
            .Map(dest => dest.SuggestedCategoryName, src => src.AiAnalysis) // We'll fill this in later
            .AfterMapping((src, dest) =>
            {
                if (src.AiAnalysis?.SuggestedCategoryId != null)
                {
                    dest.SuggestedCategoryName = $"Category #{src.AiAnalysis.SuggestedCategoryId}";
                }
            });

        config.NewConfig<StatusChange, StatusChangeResponse>()
            .Map(dest => dest.OldStatus, src => src.OldStatus.ToString())
            .Map(dest => dest.NewStatus, src => src.NewStatus.ToString());

        config.NewConfig<CreateFeedbackRequest, Feedback>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.StatusHistory)
            .Ignore(dest => dest.AiAnalysis!)
            .Ignore(dest => dest.VoteCount)
            .Ignore(dest => dest.VoterIds)
            .Ignore(dest => dest.AssignedTo!)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt!)
            .Ignore(dest => dest.CompletedAt!)
            .Ignore(dest => dest.AttachmentUrls);
    }
}