using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Admin.DTOs;
using FluentValidation;
using MediatR;

namespace AcademicGPA.Application.Features.Admin;

public record EditStudentInfoCommand(Guid StudentId, EditStudentInfoDto Dto, string AdminIpAddress) : IRequest;
public record LockStudentCommand(Guid StudentId, string Reason, string AdminIpAddress) : IRequest;
public record UnlockStudentCommand(Guid StudentId, string AdminIpAddress) : IRequest;
public record DeleteStudentCommand(Guid StudentId, string AdminIpAddress) : IRequest;
public record ResetStudentPasswordCommand(Guid StudentId, string AdminIpAddress) : IRequest<string>;
public record SendDirectNotificationCommand(Guid AdminUserId, Guid RecipientId, string Title, string Message, string Type) : IRequest;
public record BroadcastNotificationCommand(Guid AdminUserId, string Title, string Message) : IRequest;

// Validators
public class EditStudentInfoCommandValidator : AbstractValidator<EditStudentInfoCommand>
{
    public EditStudentInfoCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.Dto.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Dto.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Dto.StudentCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Dto.UniversityName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.MajorName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.EnrollmentYear).GreaterThan(2000);
        RuleFor(x => x.Dto.TotalRequiredCredits).GreaterThan(0);
    }
}

public class LockStudentCommandValidator : AbstractValidator<LockStudentCommand>
{
    public LockStudentCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class SendDirectNotificationCommandValidator : AbstractValidator<SendDirectNotificationCommand>
{
    public SendDirectNotificationCommandValidator()
    {
        RuleFor(x => x.RecipientId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
    }
}

public class BroadcastNotificationCommandValidator : AbstractValidator<BroadcastNotificationCommand>
{
    public BroadcastNotificationCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}

// Handlers

public class EditStudentInfoCommandHandler : IRequestHandler<EditStudentInfoCommand>
{
    private readonly IAdminService _adminService;
    public EditStudentInfoCommandHandler(IAdminService adminService) => _adminService = adminService;
    public async Task Handle(EditStudentInfoCommand request, CancellationToken cancellationToken)
    {
        await _adminService.EditStudentInfoAsync(request.StudentId, request.Dto, request.AdminIpAddress, cancellationToken);
    }
}

public class LockStudentCommandHandler : IRequestHandler<LockStudentCommand>
{
    private readonly IAdminService _adminService;
    public LockStudentCommandHandler(IAdminService adminService) => _adminService = adminService;
    public async Task Handle(LockStudentCommand request, CancellationToken cancellationToken)
    {
        await _adminService.LockStudentAsync(request.StudentId, request.Reason, request.AdminIpAddress, cancellationToken);
    }
}

public class UnlockStudentCommandHandler : IRequestHandler<UnlockStudentCommand>
{
    private readonly IAdminService _adminService;
    public UnlockStudentCommandHandler(IAdminService adminService) => _adminService = adminService;
    public async Task Handle(UnlockStudentCommand request, CancellationToken cancellationToken)
    {
        await _adminService.UnlockStudentAsync(request.StudentId, request.AdminIpAddress, cancellationToken);
    }
}

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand>
{
    private readonly IAdminService _adminService;
    public DeleteStudentCommandHandler(IAdminService adminService) => _adminService = adminService;
    public async Task Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        await _adminService.DeleteStudentAsync(request.StudentId, request.AdminIpAddress, cancellationToken);
    }
}

public class ResetStudentPasswordCommandHandler : IRequestHandler<ResetStudentPasswordCommand, string>
{
    private readonly IAdminService _adminService;
    public ResetStudentPasswordCommandHandler(IAdminService adminService) => _adminService = adminService;
    public Task<string> Handle(ResetStudentPasswordCommand request, CancellationToken cancellationToken)
        => _adminService.ResetStudentPasswordAsync(request.StudentId, request.AdminIpAddress, cancellationToken);
}

public class SendDirectNotificationCommandHandler : IRequestHandler<SendDirectNotificationCommand>
{
    private readonly IAdminService _adminService;
    public SendDirectNotificationCommandHandler(IAdminService adminService) => _adminService = adminService;
    public async Task Handle(SendDirectNotificationCommand request, CancellationToken cancellationToken)
    {
        await _adminService.SendDirectNotificationAsync(request.AdminUserId, request.RecipientId, request.Title, request.Message, request.Type, cancellationToken);
    }
}

public class BroadcastNotificationCommandHandler : IRequestHandler<BroadcastNotificationCommand>
{
    private readonly IAdminService _adminService;
    public BroadcastNotificationCommandHandler(IAdminService adminService) => _adminService = adminService;
    public async Task Handle(BroadcastNotificationCommand request, CancellationToken cancellationToken)
    {
        await _adminService.BroadcastNotificationAsync(request.AdminUserId, request.Title, request.Message, cancellationToken);
    }
}
