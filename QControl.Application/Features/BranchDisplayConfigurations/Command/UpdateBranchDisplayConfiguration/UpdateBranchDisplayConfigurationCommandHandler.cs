using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Command.UpdateBranchDisplayConfiguration;

internal sealed class UpdateBranchDisplayConfigurationCommandHandler
    : ICommandHandler<UpdateBranchDisplayConfigurationCommand, BranchDisplayConfigurationResponse>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<BranchDisplayConfiguration> _configurations;
    private readonly IWriteRepository<BranchDisplayConfiguration> _writer;
    private readonly IConcurrencyTokenManager _concurrency;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBranchDisplayConfigurationCommandHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<BranchDisplayConfiguration> configurations,
        IWriteRepository<BranchDisplayConfiguration> writer,
        IConcurrencyTokenManager concurrency,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _branches = branches;
        _configurations = configurations;
        _writer = writer;
        _concurrency = concurrency;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BranchDisplayConfigurationResponse>> Handle(
        UpdateBranchDisplayConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure("BranchDisplayConfiguration.Update.Unauthenticated", ErrorMessage.Branch_Authentication_Required, ErrorType.Unauthorized);
        }
        if (!await _branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken))
        {
            return Failure("BranchDisplayConfiguration.BranchNotFound", ErrorMessage.Branch_NotFound, ErrorType.NotFound);
        }

        var configuration = await _configurations.FirstOrDefaultAsync(
            new GetBranchDisplayConfigurationForUpdateSpec(request.BranchId),
            cancellationToken);
        if (configuration is null)
        {
            return Failure("BranchDisplayConfiguration.NotFound", BranchDisplayFeatureMessages.ConfigurationNotFound, ErrorType.NotFound);
        }
        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Failure("BranchDisplayConfiguration.InvalidRowVersion", ErrorMessage.RowVersion_Invalid, ErrorType.Validation);
        }

        _concurrency.SetOriginalRowVersion(configuration, rowVersion);
        configuration.Update(
            BranchDisplayConfigurationSettingsFactory.FromInput(request),
            _currentUser.UserId.Value);
        _writer.Update(configuration);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failure("BranchDisplayConfiguration.ConcurrencyConflict", ErrorMessage.Concurrency_Conflict, ErrorType.Conflict);
        }

        return Result<BranchDisplayConfigurationResponse>.Ok(
            BranchDisplayConfigurationResponseFactory.FromEntity(configuration));
    }

    private static Result<BranchDisplayConfigurationResponse> Failure(
        string code,
        string message,
        ErrorType errorType) =>
        Result<BranchDisplayConfigurationResponse>.Fail(new Error(code, message, errorType));
}
