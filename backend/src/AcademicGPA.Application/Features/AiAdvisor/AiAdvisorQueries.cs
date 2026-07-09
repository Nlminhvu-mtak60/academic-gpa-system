using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.AiAdvisor.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.AiAdvisor;

public record GetConversationsQuery(int Page = 1, int PageSize = 20) : IRequest<(IReadOnlyList<ConversationDto> Items, int TotalCount)>;

public record GetMessagesQuery(Guid ConversationId) : IRequest<IReadOnlyList<ConversationMessageDto>>;

public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, (IReadOnlyList<ConversationDto> Items, int TotalCount)>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetConversationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<(IReadOnlyList<ConversationDto> Items, int TotalCount)> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var query = _context.Conversations
            .Where(c => c.UserId == userId && !c.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ConversationDto(c.Id, c.Title, c.CreatedAt, c.UpdatedAt))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, IReadOnlyList<ConversationMessageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMessagesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<ConversationMessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId && !c.IsDeleted, cancellationToken);

        if (conversation == null)
        {
            throw new NotFoundException("Conversation", request.ConversationId);
        }

        if (conversation.UserId != userId)
        {
            throw new ForbiddenException("You are not authorized to view messages in this conversation.");
        }

        var messages = await _context.ConversationMessages
            .Where(m => m.ConversationId == request.ConversationId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ConversationMessageDto(m.Role, m.Content, m.CreatedAt))
            .ToListAsync(cancellationToken);

        return messages;
    }
}
