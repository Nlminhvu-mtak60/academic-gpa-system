using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Students.Commands.UploadAvatar;

public record UploadAvatarCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileLength
) : IRequest<string>;

public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UploadAvatarCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<string> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        // 1. Get Current User ID
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        // 2. Validate File Length (Max 2MB = 2097152 bytes)
        if (request.FileLength > 2097152)
        {
            throw new AcademicGPA.Application.Common.Exceptions.ValidationException("File", "File size cannot exceed 2MB.");
        }

        // 3. Validate Content Type
        var allowedContentTypes = new[] { "image/png", "image/jpeg", "image/jpg", "image/webp" };
        if (!allowedContentTypes.Contains(request.ContentType.ToLower()))
        {
            throw new AcademicGPA.Application.Common.Exceptions.ValidationException("File", "Only PNG, JPG, JPEG, and WebP images are allowed.");
        }

        // 4. Generate unique file name and save to local wwwroot path
        var fileExtension = Path.GetExtension(request.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        
        // Define directory root (in a real API, maps to WebRootPath)
        var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
        Directory.CreateDirectory(uploadFolder);

        var filePath = Path.Combine(uploadFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.FileStream.CopyToAsync(stream, cancellationToken);
        }

        // 5. Update user AvatarUrl
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        var avatarUrl = $"/uploads/avatars/{uniqueFileName}";
        user.AvatarUrl = avatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return avatarUrl;
    }
}
