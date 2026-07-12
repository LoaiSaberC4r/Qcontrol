using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Identity;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Security;
using QControl.Domain.Entities;
using QControl.Domain.Identity;

namespace Qcontrol.Application.Features.BranchAdmins.Command.CreateBranchAdmin;

internal sealed class CreateBranchAdminCommandHandler
    : ICommandHandler<CreateBranchAdminCommand, CreateBranchAdminResponse>
{
    private const string BranchAdministratorRoleName = "Branch Administrator";

    private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
    private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
    private readonly IWriteReadRepository<TechnicalAdmin> _technicalAdminReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Role> _roleReadRepository;
    private readonly IWriteRepository<BranchAdmin> _branchAdminWriteRepository;
    private readonly IWriteRepository<UserRole> _userRoleWriteRepository;
    private readonly IWriteRepository<ApplicationUserBranch> _applicationUserBranchWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PasswordHasher<ApplicationUser> _passwordHasher;

    public CreateBranchAdminCommandHandler(
        IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
        IWriteRepository<ApplicationUser> applicationUserWriteRepository,
        IWriteReadRepository<TechnicalAdmin> technicalAdminReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Role> roleReadRepository,
        IWriteRepository<BranchAdmin> branchAdminWriteRepository,
        IWriteRepository<UserRole> userRoleWriteRepository,
        IWriteRepository<ApplicationUserBranch> applicationUserBranchWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _applicationUserReadRepository = applicationUserReadRepository
            ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
        _applicationUserWriteRepository = applicationUserWriteRepository
            ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));
        _technicalAdminReadRepository = technicalAdminReadRepository
            ?? throw new ArgumentNullException(nameof(technicalAdminReadRepository));
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _roleReadRepository = roleReadRepository
            ?? throw new ArgumentNullException(nameof(roleReadRepository));
        _branchAdminWriteRepository = branchAdminWriteRepository
            ?? throw new ArgumentNullException(nameof(branchAdminWriteRepository));
        _userRoleWriteRepository = userRoleWriteRepository
            ?? throw new ArgumentNullException(nameof(userRoleWriteRepository));
        _applicationUserBranchWriteRepository = applicationUserBranchWriteRepository
            ?? throw new ArgumentNullException(nameof(applicationUserBranchWriteRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));

        _passwordHasher = new PasswordHasher<ApplicationUser>();
    }

    public async Task<Result<CreateBranchAdminResponse>> Handle(
        CreateBranchAdminCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "BranchAdmins.Create.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required,
                ErrorType.Unauthorized);
        }

        if (_currentUser.UserTypeValue != (int)QControl.Domain.Enums.UserType.TechnicalAdmin)
        {
            return Failure(
                "BranchAdmins.Create.ForbiddenActorType",
                IdentityFeatureMessages.ForbiddenActorType,
                ErrorType.Security);
        }

        var currentUserId = _currentUser.UserId.Value;

        var technicalAdminProfileExists =
            await _technicalAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentUserId,
                cancellationToken);

        if (!technicalAdminProfileExists)
        {
            return Failure(
                "BranchAdmins.Create.ForbiddenActorType",
                IdentityFeatureMessages.ForbiddenActorType,
                ErrorType.Security);
        }

        var branch =
            await _branchReadRepository.GetByIdAsync(
                request.BranchId,
                cancellationToken);

        if (branch is null)
        {
            return Failure(
                "BranchAdmins.Create.BranchNotFound",
                ErrorMessage.Branch_NotFound,
                ErrorType.NotFound);
        }

        if (!branch.IsActive)
        {
            return Failure(
                "BranchAdmins.Create.BranchInactive",
                IdentityFeatureMessages.BranchInactive,
                ErrorType.Security);
        }

        var normalizedUserName = request.UserName.Trim();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var userNameAlreadyExists =
            await _applicationUserReadRepository.AnyAsync(
                x => x.UserName == normalizedUserName,
                cancellationToken);

        if (userNameAlreadyExists)
        {
            return Failure(
                "BranchAdmins.Create.UserNameAlreadyExists",
                IdentityFeatureMessages.UserNameAlreadyExists,
                ErrorType.Conflict);
        }

        var emailAlreadyExists =
            await _applicationUserReadRepository.AnyAsync(
                x => x.Email == normalizedEmail,
                cancellationToken);

        if (emailAlreadyExists)
        {
            return Failure(
                "BranchAdmins.Create.EmailAlreadyExists",
                IdentityFeatureMessages.EmailAlreadyExists,
                ErrorType.Conflict);
        }

        var role =
            await _roleReadRepository.GetByPropertyAsync(
                x => x.Name == BranchAdministratorRoleName,
                cancellationToken);

        if (role is null)
        {
            return Failure(
                "BranchAdmins.Create.RoleNotFound",
                IdentityFeatureMessages.RoleNotFound,
                ErrorType.Infrastructure);
        }

        var applicationUser = ApplicationUser.CreateBranchAdmin(
            userName: normalizedUserName,
            email: normalizedEmail,
            nameEn: request.NameEn,
            nameAr: request.NameAr,
            phoneNumber: request.PhoneNumber,
            createdByApplicationUserId: currentUserId);

        var passwordHash =
            _passwordHasher.HashPassword(
                applicationUser,
                request.TemporaryPassword);

        applicationUser.SetPasswordHash(passwordHash);

        var branchAdmin = BranchAdmin.Create(
            applicationUser.Id,
            currentUserId);

        var userRole = UserRole.Create(
            applicationUser.Id,
            role.Id,
            currentUserId);

        var applicationUserBranch = ApplicationUserBranch.Create(
            applicationUser.Id,
            request.BranchId,
            currentUserId);

        await _applicationUserWriteRepository.AddAsync(
            applicationUser,
            cancellationToken);
        await _branchAdminWriteRepository.AddAsync(
            branchAdmin,
            cancellationToken);
        await _userRoleWriteRepository.AddAsync(
            userRole,
            cancellationToken);
        await _applicationUserBranchWriteRepository.AddAsync(
            applicationUserBranch,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            BranchAdminUniqueConstraintErrorMapper.TryMap(exception, out var error))
        {
            return Result<CreateBranchAdminResponse>.Fail(error);
        }

        return Result<CreateBranchAdminResponse>.Ok(
            new CreateBranchAdminResponse
            {
                ApplicationUserId = applicationUser.Id,
                BranchAdminId = branchAdmin.Id,
                BranchId = request.BranchId,
                UserName = applicationUser.UserName,
                Email = applicationUser.Email,
                NameEn = applicationUser.NameEn,
                NameAr = applicationUser.NameAr,
                PhoneNumber = applicationUser.PhoneNumber,
                UserType = applicationUser.UserType.ToString(),
                RoleName = role.Name,
                IsActive = applicationUser.IsActive,
                IsFirstLogin = applicationUser.IsFirstLogin,
                CreatedByApplicationUserId = applicationUser.CreatedByApplicationUserId,
                CreatedOnUtc = applicationUser.CreatedOnUtc,
                Message = IdentityFeatureMessages.BranchAdminCreated
            });
    }

    private static Result<CreateBranchAdminResponse> Failure(
        string code,
        string message,
        ErrorType type)
    {
        return Result<CreateBranchAdminResponse>.Fail(
            new Error(
                Code: code,
                Message: message,
                Type: type));
    }
}
