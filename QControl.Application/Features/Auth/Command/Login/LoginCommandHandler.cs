using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Qcontrol.Domain.Identity;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Options;
using QControl.Application.Shared.Dto;
using QControl.Domain.Enums;
using QControl.Domain.Identity;

namespace Qcontrol.Application.Features.Auth.Command.Login;

internal sealed class LoginCommandHandler
    : ICommandHandler<LoginCommand, UserTokenDto>
{
    private readonly IWriteReadRepository<ApplicationUser>
        _applicationUserReadRepository;

    private readonly IWriteReadRepository<TechnicalAdmin>
        _technicalAdminReadRepository;

    private readonly IWriteReadRepository<UserRole>
        _userRoleReadRepository;

    private readonly IWriteReadRepository<RolePermission>
        _rolePermissionReadRepository;

    private readonly IJwtProvider _jwtProvider;

    private readonly PasswordPolicyOptions _passwordPolicyOptions;

    private readonly PasswordHasher<ApplicationUser> _passwordHasher;

    public LoginCommandHandler(
        IWriteReadRepository<ApplicationUser>
            applicationUserReadRepository,
        IWriteReadRepository<TechnicalAdmin>
            technicalAdminReadRepository,
        IWriteReadRepository<UserRole>
            userRoleReadRepository,
        IWriteReadRepository<RolePermission>
            rolePermissionReadRepository,
        IJwtProvider jwtProvider,
        IOptions<PasswordPolicyOptions>
            passwordPolicyOptions)
    {
        _applicationUserReadRepository =
            applicationUserReadRepository
            ?? throw new ArgumentNullException(
                nameof(applicationUserReadRepository));

        _technicalAdminReadRepository =
            technicalAdminReadRepository
            ?? throw new ArgumentNullException(
                nameof(technicalAdminReadRepository));

        _userRoleReadRepository =
            userRoleReadRepository
            ?? throw new ArgumentNullException(
                nameof(userRoleReadRepository));

        _rolePermissionReadRepository =
            rolePermissionReadRepository
            ?? throw new ArgumentNullException(
                nameof(rolePermissionReadRepository));

        _jwtProvider =
            jwtProvider
            ?? throw new ArgumentNullException(
                nameof(jwtProvider));

        _passwordPolicyOptions =
            passwordPolicyOptions?.Value
            ?? throw new ArgumentNullException(
                nameof(passwordPolicyOptions));

        _passwordHasher =
            new PasswordHasher<ApplicationUser>();
    }

    public async Task<Result<UserTokenDto>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var userNameOrEmail =
            request.UserNameOrEmail.Trim();

        var applicationUser =
            await _applicationUserReadRepository
                .FirstOrDefaultAsync(
                    new GetApplicationUserForLoginSpec(
                        userNameOrEmail),
                    cancellationToken);

        if (applicationUser is null)
        {
            return InvalidCredentials();
        }

        var passwordVerificationResult =
            _passwordHasher.VerifyHashedPassword(
                applicationUser,
                applicationUser.PasswordHash,
                request.Password);

        if (passwordVerificationResult ==
            PasswordVerificationResult.Failed)
        {
            return InvalidCredentials();
        }

        var actorProfileExists =
            await ActorProfileExistsAsync(
                applicationUser.UserType,
                applicationUser.Id,
                cancellationToken);

        if (!actorProfileExists)
        {
            return Result<UserTokenDto>.Fail(
                new Error(
                    Code:
                        "Auth.Login.ActorProfileNotFound",

                    Message:
                        ErrorMessage
                            .Login_ActorProfile_NotFound,

                    Type:
                        ErrorType.NotFound));
        }

        var userRoles =
            await _userRoleReadRepository.ListAsync(
                new GetUserRolesForLoginSpec(
                    applicationUser.Id),
                cancellationToken);

        if (userRoles.Count == 0)
        {
            return Result<UserTokenDto>.Fail(
                new Error(
                    Code:
                        "Auth.Login.UserRolesNotFound",

                    Message:
                        ErrorMessage
                            .Login_UserRoles_NotFound,

                    Type:
                        ErrorType.Security));
        }

        var roleIds = userRoles
            .Select(x => x.RoleId)
            .Distinct()
            .ToArray();

        var roleNames = userRoles
            .Select(x => x.RoleName)
            .Where(x =>
                !string.IsNullOrWhiteSpace(x))
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var userPermissions =
            await _rolePermissionReadRepository.ListAsync(
                new GetUserPermissionsForLoginSpec(
                    roleIds),
                cancellationToken);

        var permissionNames = userPermissions
            .Select(x => x.PermissionName)
            .Where(x =>
                !string.IsNullOrWhiteSpace(x))
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var token = await _jwtProvider.Generate(
            userId:
                applicationUser.Id,

            email:
                applicationUser.Email,

            phoneNumber:
                applicationUser.PhoneNumber
                ?? string.Empty,

            roleNames:
                roleNames,

            userType:
                applicationUser.UserType,

            permissions:
                permissionNames,

            activeBranchId:
                null,

            cancellationToken:
                cancellationToken);

        var passwordExpiresOnUtc =
            applicationUser.PasswordChangedOnUtc
                .AddDays(
                    _passwordPolicyOptions.ExpiryDays);

        token = token with
        {
            RequiresBranchSelection = false,

            ActiveBranchId = null,

            Branches =
                Array.Empty<LoginBranchSelectionItemResponse>(),

            FirstLoginFlag =
                applicationUser.IsFirstLogin,

            PasswordChangedOnUtc =
                applicationUser.PasswordChangedOnUtc,

            PasswordExpiresOnUtc =
                passwordExpiresOnUtc,

            PasswordExpiredFlag =
                passwordExpiresOnUtc <= DateTime.UtcNow
        };

        return Result<UserTokenDto>.Ok(token);
    }

    private async Task<bool> ActorProfileExistsAsync(
        UserType userType,
        Guid applicationUserId,
        CancellationToken cancellationToken)
    {
        return userType switch
        {
            UserType.TechnicalAdmin =>
                await _technicalAdminReadRepository.AnyAsync(
                    x =>
                        x.ApplicationUserId ==
                        applicationUserId,
                    cancellationToken),

            _ => false
        };
    }

    private static Result<UserTokenDto>
        InvalidCredentials()
    {
        return Result<UserTokenDto>.Fail(
            new Error(
                Code:
                    "Auth.Login.InvalidCredentials",

                Message:
                    ErrorMessage.Login_InvalidCredentials,

                Type:
                    ErrorType.Unauthorized));
    }
}
