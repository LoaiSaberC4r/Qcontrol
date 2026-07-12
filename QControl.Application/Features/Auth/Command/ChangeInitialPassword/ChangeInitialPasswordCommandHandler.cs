using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Identity;
using Qcontrol.Application.Features.Auth.Shared;
using Qcontrol.Domain.Identity;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Dto;
using QControl.Application.Shared.Security;

namespace Qcontrol.Application.Features.Auth.Command.ChangeInitialPassword;

internal sealed class ChangeInitialPasswordCommandHandler
    : ICommandHandler<ChangeInitialPasswordCommand, UserTokenDto>
{
    private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
    private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentTokenContext _currentTokenContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserTokenFactory _userTokenFactory;
    private readonly PasswordHasher<ApplicationUser> _passwordHasher;

    public ChangeInitialPasswordCommandHandler(
        IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
        IWriteRepository<ApplicationUser> applicationUserWriteRepository,
        ICurrentUser currentUser,
        ICurrentTokenContext currentTokenContext,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork,
        UserTokenFactory userTokenFactory)
    {
        _applicationUserReadRepository = applicationUserReadRepository
            ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
        _applicationUserWriteRepository = applicationUserWriteRepository
            ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _currentTokenContext = currentTokenContext
            ?? throw new ArgumentNullException(nameof(currentTokenContext));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
        _userTokenFactory = userTokenFactory
            ?? throw new ArgumentNullException(nameof(userTokenFactory));
        _passwordHasher = new PasswordHasher<ApplicationUser>();
    }

    public async Task<Result<UserTokenDto>> Handle(
        ChangeInitialPasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "Auth.ChangeInitialPassword.Unauthenticated",
                IdentityFeatureMessages.ChangeInitialPasswordUnauthenticated,
                ErrorType.Unauthorized);
        }

        if (!_currentTokenContext.PasswordChangeRequired)
        {
            return Failure(
                "Auth.ChangeInitialPassword.InvalidTokenPurpose",
                IdentityFeatureMessages.ChangeInitialPasswordInvalidTokenPurpose,
                ErrorType.Security);
        }

        var applicationUser =
            await _applicationUserReadRepository.GetByIdTrackedAsync(
                _currentUser.UserId.Value,
                cancellationToken);

        if (applicationUser is null)
        {
            return Failure(
                "Auth.ChangeInitialPassword.UserNotFound",
                IdentityFeatureMessages.ChangeInitialPasswordUserNotFound,
                ErrorType.NotFound);
        }

        if (!applicationUser.IsActive)
        {
            return Failure(
                "Auth.ChangeInitialPassword.UserInactive",
                IdentityFeatureMessages.ChangeInitialPasswordUserInactive,
                ErrorType.Security);
        }

        if (!applicationUser.IsFirstLogin)
        {
            return Failure(
                "Auth.ChangeInitialPassword.NotRequired",
                IdentityFeatureMessages.ChangeInitialPasswordNotRequired,
                ErrorType.Conflict);
        }

        var samePasswordVerificationResult =
            _passwordHasher.VerifyHashedPassword(
                applicationUser,
                applicationUser.PasswordHash,
                request.NewPassword);

        if (samePasswordVerificationResult != PasswordVerificationResult.Failed)
        {
            return Failure(
                "Auth.ChangeInitialPassword.SameAsTemporaryPassword",
                IdentityFeatureMessages.SameAsTemporaryPassword,
                ErrorType.Conflict);
        }

        var newPasswordHash =
            _passwordHasher.HashPassword(
                applicationUser,
                request.NewPassword);

        applicationUser.ChangePassword(
            newPasswordHash,
            _dateTimeProvider.UtcNow);

        _applicationUserWriteRepository.Update(applicationUser);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _userTokenFactory.CreateAsync(
            applicationUser,
            passwordChangeRequired: false,
            cancellationToken);
    }

    private static Result<UserTokenDto> Failure(
        string code,
        string message,
        ErrorType type)
    {
        return Result<UserTokenDto>.Fail(
            new Error(
                Code: code,
                Message: message,
                Type: type));
    }
}
