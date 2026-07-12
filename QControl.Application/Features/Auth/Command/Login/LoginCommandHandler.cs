using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Identity;
using Qcontrol.Application.Features.Auth.Shared;
using Qcontrol.Domain.Identity;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Dto;

namespace Qcontrol.Application.Features.Auth.Command.Login;

internal sealed class LoginCommandHandler
    : ICommandHandler<LoginCommand, UserTokenDto>
{
    private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
    private readonly UserTokenFactory _userTokenFactory;
    private readonly PasswordHasher<ApplicationUser> _passwordHasher;

    public LoginCommandHandler(
        IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
        UserTokenFactory userTokenFactory)
    {
        _applicationUserReadRepository = applicationUserReadRepository
            ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
        _userTokenFactory = userTokenFactory
            ?? throw new ArgumentNullException(nameof(userTokenFactory));
        _passwordHasher = new PasswordHasher<ApplicationUser>();
    }

    public async Task<Result<UserTokenDto>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var userNameOrEmail = request.UserNameOrEmail.Trim();

        var applicationUser =
            await _applicationUserReadRepository.FirstOrDefaultAsync(
                new GetApplicationUserForLoginSpec(userNameOrEmail),
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

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            return InvalidCredentials();
        }

        return await _userTokenFactory.CreateAsync(
            applicationUser,
            passwordChangeRequired: applicationUser.IsFirstLogin,
            cancellationToken);
    }

    private static Result<UserTokenDto> InvalidCredentials()
    {
        return Result<UserTokenDto>.Fail(
            new Error(
                Code: "Auth.Login.InvalidCredentials",
                Message: ErrorMessage.Login_InvalidCredentials,
                Type: ErrorType.Unauthorized));
    }
}
