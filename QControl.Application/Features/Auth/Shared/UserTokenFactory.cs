using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.Specification;
using Microsoft.Extensions.Options;
using Qcontrol.Application.Features.Auth.Command.Login;
using Qcontrol.Domain.Identity;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Options;
using QControl.Application.Shared.Dto;
using QControl.Application.Shared.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;
using QControl.Domain.Identity;

namespace Qcontrol.Application.Features.Auth.Shared;

internal sealed class UserTokenFactory
{
    private readonly IWriteReadRepository<TechnicalAdmin> _technicalAdminReadRepository;
    private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
    private readonly IWriteReadRepository<ApplicationUserBranch> _applicationUserBranchReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<UserRole> _userRoleReadRepository;
    private readonly IWriteReadRepository<RolePermission> _rolePermissionReadRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly PasswordPolicyOptions _passwordPolicyOptions;

    public UserTokenFactory(
        IWriteReadRepository<TechnicalAdmin> technicalAdminReadRepository,
        IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
        IWriteReadRepository<ApplicationUserBranch> applicationUserBranchReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<UserRole> userRoleReadRepository,
        IWriteReadRepository<RolePermission> rolePermissionReadRepository,
        IJwtProvider jwtProvider,
        IOptions<PasswordPolicyOptions> passwordPolicyOptions)
    {
        _technicalAdminReadRepository = technicalAdminReadRepository
            ?? throw new ArgumentNullException(nameof(technicalAdminReadRepository));
        _branchAdminReadRepository = branchAdminReadRepository
            ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));
        _applicationUserBranchReadRepository = applicationUserBranchReadRepository
            ?? throw new ArgumentNullException(nameof(applicationUserBranchReadRepository));
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _userRoleReadRepository = userRoleReadRepository
            ?? throw new ArgumentNullException(nameof(userRoleReadRepository));
        _rolePermissionReadRepository = rolePermissionReadRepository
            ?? throw new ArgumentNullException(nameof(rolePermissionReadRepository));
        _jwtProvider = jwtProvider
            ?? throw new ArgumentNullException(nameof(jwtProvider));
        _passwordPolicyOptions = passwordPolicyOptions?.Value
            ?? throw new ArgumentNullException(nameof(passwordPolicyOptions));
    }

    public async Task<Result<UserTokenDto>> CreateAsync(
        ApplicationUser applicationUser,
        bool passwordChangeRequired,
        CancellationToken cancellationToken)
    {
        var actorProfileResult = await ValidateActorProfileAsync(
            applicationUser,
            cancellationToken);

        if (actorProfileResult.IsFailure)
        {
            return Result<UserTokenDto>.Fail(actorProfileResult.Errors);
        }

        var branchContextResult = await ResolveBranchContextAsync(
            applicationUser,
            cancellationToken);

        if (branchContextResult.IsFailure)
        {
            return Result<UserTokenDto>.Fail(branchContextResult.Errors);
        }

        var branchContext = branchContextResult.Value;

        var userRoles =
            await _userRoleReadRepository.ListAsync(
                new GetUserRolesForLoginSpec(applicationUser.Id),
                cancellationToken);

        if (userRoles.Count == 0)
        {
            return Result<UserTokenDto>.Fail(
                new Error(
                    Code: "Auth.Login.UserRolesNotFound",
                    Message: ErrorMessage.Login_UserRoles_NotFound,
                    Type: ErrorType.Security));
        }

        var roleIds = userRoles
            .Select(x => x.RoleId)
            .Distinct()
            .ToArray();

        var roleNames = userRoles
            .Select(x => x.RoleName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissionNames = Array.Empty<string>();

        if (!passwordChangeRequired)
        {
            var userPermissions =
                await _rolePermissionReadRepository.ListAsync(
                    new GetUserPermissionsForLoginSpec(roleIds),
                    cancellationToken);

            permissionNames = userPermissions
                .Select(x => x.PermissionName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        var token = await _jwtProvider.Generate(
            userId: applicationUser.Id,
            email: applicationUser.Email,
            phoneNumber: applicationUser.PhoneNumber ?? string.Empty,
            roleNames: roleNames,
            userType: applicationUser.UserType,
            permissions: permissionNames,
            activeBranchId: branchContext.ActiveBranchId,
            passwordChangeRequired: passwordChangeRequired,
            cancellationToken: cancellationToken);

        var passwordExpiresOnUtc =
            applicationUser.PasswordChangedOnUtc
                .AddDays(_passwordPolicyOptions.ExpiryDays);

        return Result<UserTokenDto>.Ok(token with
        {
            RequiresBranchSelection = false,
            ActiveBranchId = branchContext.ActiveBranchId,
            Branches = branchContext.Branches,
            FirstLoginFlag = applicationUser.IsFirstLogin,
            PasswordChangedOnUtc = applicationUser.PasswordChangedOnUtc,
            PasswordExpiresOnUtc = passwordExpiresOnUtc,
            PasswordExpiredFlag =
                !applicationUser.IsFirstLogin &&
                passwordExpiresOnUtc <= DateTime.UtcNow
        });
    }

    private async Task<Result> ValidateActorProfileAsync(
        ApplicationUser applicationUser,
        CancellationToken cancellationToken)
    {
        return applicationUser.UserType switch
        {
            UserType.TechnicalAdmin =>
                await _technicalAdminReadRepository.AnyAsync(
                    x => x.ApplicationUserId == applicationUser.Id,
                    cancellationToken)
                    ? Result.Ok()
                    : Result.Fail(new Error(
                        Code: "Auth.Login.ActorProfileNotFound",
                        Message: ErrorMessage.Login_ActorProfile_NotFound,
                        Type: ErrorType.NotFound)),

            UserType.BranchAdmin =>
                await _branchAdminReadRepository.AnyAsync(
                    x => x.ApplicationUserId == applicationUser.Id,
                    cancellationToken)
                    ? Result.Ok()
                    : Result.Fail(new Error(
                        Code: "Auth.Login.BranchAdminProfileNotFound",
                        Message: IdentityFeatureMessages.BranchAdminProfileNotFound,
                        Type: ErrorType.NotFound)),

            _ => Result.Fail(new Error(
                Code: "Auth.Login.UnsupportedUserType",
                Message: ErrorMessage.Login_ActorProfile_NotFound,
                Type: ErrorType.Security))
        };
    }

    private async Task<Result<TokenBranchContext>> ResolveBranchContextAsync(
        ApplicationUser applicationUser,
        CancellationToken cancellationToken)
    {
        if (applicationUser.UserType == UserType.TechnicalAdmin)
        {
            return Result<TokenBranchContext>.Ok(
                new TokenBranchContext(
                    null,
                    Array.Empty<LoginBranchSelectionItemResponse>()));
        }

        if (applicationUser.UserType != UserType.BranchAdmin)
        {
            return Result<TokenBranchContext>.Fail(new Error(
                Code: "Auth.Login.UnsupportedUserType",
                Message: ErrorMessage.Login_ActorProfile_NotFound,
                Type: ErrorType.Security));
        }

        var assignments =
            await _applicationUserBranchReadRepository.ListAsync(
                new GetBranchAssignmentsForLoginSpec(applicationUser.Id),
                cancellationToken);

        if (assignments.Count == 0)
        {
            return Result<TokenBranchContext>.Fail(new Error(
                Code: "Auth.Login.BranchAssignmentNotFound",
                Message: IdentityFeatureMessages.BranchAssignmentNotFound,
                Type: ErrorType.Infrastructure));
        }

        if (assignments.Count > 1)
        {
            return Result<TokenBranchContext>.Fail(new Error(
                Code: "Auth.Login.MultipleBranchAssignmentsNotSupported",
                Message: IdentityFeatureMessages.MultipleBranchAssignmentsNotSupported,
                Type: ErrorType.Conflict));
        }

        var branchId = assignments[0].BranchId;
        var branch =
            await _branchReadRepository.FirstOrDefaultAsync(
                new GetLoginBranchSelectionItemSpec(branchId),
                cancellationToken);

        if (branch is null)
        {
            return Result<TokenBranchContext>.Fail(new Error(
                Code: "Auth.Login.BranchNotFound",
                Message: IdentityFeatureMessages.LoginBranchNotFound,
                Type: ErrorType.Infrastructure));
        }

        if (!branch.IsActive)
        {
            return Result<TokenBranchContext>.Fail(new Error(
                Code: "Auth.Login.BranchInactive",
                Message: IdentityFeatureMessages.LoginBranchInactive,
                Type: ErrorType.Security));
        }

        return Result<TokenBranchContext>.Ok(
            new TokenBranchContext(
                branch.BranchId,
                new[] { branch }));
    }

    private sealed record TokenBranchContext(
        int? ActiveBranchId,
        IReadOnlyCollection<LoginBranchSelectionItemResponse> Branches);
}

internal sealed record BranchAssignmentForLoginDto
{
    public int BranchId { get; init; }
}

internal sealed class GetBranchAssignmentsForLoginSpec
    : Specification<ApplicationUserBranch, BranchAssignmentForLoginDto>
{
    public GetBranchAssignmentsForLoginSpec(Guid applicationUserId)
    {
        UseNoTracking();

        AddCriteria(x => x.ApplicationUserId == applicationUserId);

        Select(x => new BranchAssignmentForLoginDto
        {
            BranchId = x.BranchId
        });
    }
}

internal sealed class GetLoginBranchSelectionItemSpec
    : Specification<Branch, LoginBranchSelectionItemResponse>
{
    public GetLoginBranchSelectionItemSpec(int branchId)
    {
        UseNoTracking();

        AddCriteria(x => x.Id == branchId);

        Select(x => new LoginBranchSelectionItemResponse
        {
            BranchId = x.Id,
            ArabicName = x.ArabicName,
            EnglishName = x.EnglishName,
            IsActive = x.IsActive
        });
    }
}
