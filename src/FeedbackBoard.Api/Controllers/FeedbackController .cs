using FeedbackBoard.Api.Services;
using FeedbackBoard.Api.Services.Interfaces;
using FeedbackBoard.Core.Entities;
using FeedbackBoard.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedbackController(IFeedbackService feedbackService, IFeedbackMetadataService metadataService) : ControllerBase
{

    /// <summary>
    /// Створити новий фідбек
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeedbackRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "Title is required" });

        if (string.IsNullOrWhiteSpace(request.Description))
            return BadRequest(new { error = "Description is required" });

        if (request.CategoryId <= 0)
            return BadRequest(new { error = "CategoryId is required" });

        var response = await feedbackService.CreateFeedbackAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Get feedback by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var feedback = await feedbackService.GetFeedbackAsync(id);

        if (feedback == null)
            return NotFound(new { error = "Feedback not found", id });

        return Ok(feedback);
    }

    /// <summary>
    /// Get feedback by category ID
    /// </summary>
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var feedbacks = await feedbackService.GetFeedbacksByCategoryAsync(categoryId);
        return Ok(feedbacks);
    }

    /// <summary>
    /// Get feedback by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(FeedbackStatusEnum status)
    {
        var feedbacks = await feedbackService.GetFeedbacksByStatusAsync(status);
        return Ok(feedbacks);
    }

    /// <summary>
    /// Change feedback status
    /// </summary>
    [HttpPost("{id}/status")]
    public async Task<IActionResult> ChangeStatus(string id, [FromBody] ChangeStatusRequest request)
    {
        try
        {
            var response = await feedbackService.ChangeStatusAsync(
                id, request.NewStatus, request.ChangedBy, request.Reason);

            return Ok(response);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { error = "Feedback not found", id });
        }
    }

    /// <summary>
    /// Feedback voting
    /// </summary>
    [HttpPost("{id}/vote")]
    public async Task<IActionResult> Vote(string id, [FromBody] VoteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return BadRequest(new { error = "UserId is required" });

        try
        {
            var response = await feedbackService.VoteAsync(id, request.UserId);
            return Ok(response);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { error = "Feedback not found", id });
        }
    }

    /// <summary>
    /// Get all available statuses
    /// </summary>
    [HttpGet("statuses")]
    public async Task<IActionResult> GetStatuses()
    {
        var statuses = await metadataService.GetStatusesAsync();

        var result = statuses.Select(s => new
        {
            s.Id,
            s.Name,
            s.DisplayName,
            s.Color,
            s.Description
        });

        return Ok(result);
    }

    /// <summary>
    /// View all available categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await metadataService.GetCategoriesAsync();

        var result = categories.Select(c => new
        {
            c.Id,
            c.Name,
            c.DisplayName,
            c.Icon,
            c.Description
        });

        return Ok(result);
    }

    /// <summary>
    /// See the top reviews (by votes)
    /// </summary>
    [HttpGet("top")]
    public async Task<IActionResult> GetTop([FromQuery] int count = 10)
    {
        // We receive feedback with the status “New” or “Under Review”
        var newFeedbacks = await feedbackService.GetFeedbacksByStatusAsync(FeedbackStatusEnum.New);
        var underReviewFeedbacks = await feedbackService.GetFeedbacksByStatusAsync(FeedbackStatusEnum.UnderReview);

        var allOpen = newFeedbacks.Concat(underReviewFeedbacks)
            .OrderByDescending(f => f.VoteCount)
            .Take(count)
            .ToList();

        return Ok(allOpen);
    }
}