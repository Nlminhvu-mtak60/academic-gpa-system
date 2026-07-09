using System;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.AiAdvisor;
using AcademicGPA.Application.Features.AiAdvisor.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

/// <summary>
/// Handles student chat conversations, message threads, and rate-limiting audits for the AI Advisor.
/// </summary>
[ApiController]
[Route("api/v1/ai/conversations")]
[Authorize]
public class AiAdvisorController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiAdvisorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Starts a new conversation thread with the AI Advisor.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request)
    {
        var result = await _mediator.Send(new StartConversationCommand(request.Title));
        return StatusCode(StatusCodes.Status201Created, new
        {
            success = true,
            message = "Conversation started successfully.",
            data = result
        });
    }

    /// <summary>
    /// Retrieves a paginated list of the student's chat conversations.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetConversationsQuery(page, pageSize));
        var totalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize);
        return Ok(new
        {
            success = true,
            data = result.Items,
            pagination = new
            {
                page,
                pageSize,
                totalItems = result.TotalCount,
                totalPages = Math.Max(totalPages, 1)
            }
        });
    }

    /// <summary>
    /// Sends a message to the AI Advisor and returns the advisor's response.
    /// </summary>
    [HttpPost("{id:guid}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendMessageRequest request)
    {
        var result = await _mediator.Send(new SendMessageCommand(id, request.Message));
        return Ok(new
        {
            success = true,
            data = new
            {
                reply = result.Content,
                createdAt = result.CreatedAt
            }
        });
    }

    /// <summary>
    /// Retrieves the message history for a conversation.
    /// </summary>
    [HttpGet("{id:guid}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(Guid id)
    {
        var result = await _mediator.Send(new GetMessagesQuery(id));
        return Ok(new
        {
            success = true,
            data = result
        });
    }

    /// <summary>
    /// Soft-deletes/archives a conversation.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConversation(Guid id)
    {
        await _mediator.Send(new DeleteConversationCommand(id));
        return NoContent();
    }
}
