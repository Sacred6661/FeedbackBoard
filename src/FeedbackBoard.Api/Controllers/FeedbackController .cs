using FeedbackBoard.Api.Services;
using FeedbackBoard.Api.Services.Interfaces;
using FeedbackBoard.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeedbackRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required");

        var response = await _feedbackService.CreateFeedbackAsync(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var feedback = await _feedbackService.GetFeedbackAsync(id);

        if (feedback == null)
            return NotFound(new { message = "Feedback not found" });

        return Ok(feedback);
    }
}