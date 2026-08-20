using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Command.CreateBranchDisplayConfiguration;

internal sealed class CreateBranchDisplayConfigurationCommandHandler
    : ICommandHandler<CreateBranchDisplayConfigurationCommand, BranchDisplayConfigurationResponse>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<BranchDisplayConfiguration> _configurations;
    private readonly IWriteRepository<BranchDisplayConfiguration> _writer;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchDisplayConfigurationCommandHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<BranchDisplayConfiguration> configurations,
        IWriteRepository<BranchDisplayConfiguration> writer,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _branches = branches;
        _configurations = configurations;
        _writer = writer;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BranchDisplayConfigurationResponse>> Handle(
        CreateBranchDisplayConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure("BranchDisplayConfiguration.Create.Unauthenticated", ErrorMessage.Branch_Authentication_Required, ErrorType.Unauthorized);
        }
        if (!await _branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken))
        {
            return Failure("BranchDisplayConfiguration.BranchNotFound", ErrorMessage.Branch_NotFound, ErrorType.NotFound);
        }
        if (await _configurations.AnyAsync(x => x.BranchId == request.BranchId, cancellationToken))
        {
            return Failure("BranchDisplayConfiguration.AlreadyExists", BranchDisplayFeatureMessages.ConfigurationAlreadyExists, ErrorType.Conflict);
        }

        var configuration = BranchDisplayConfiguration.Create(
            request.BranchId,
            BranchDisplayConfigurationSettingsFactory.FromInput(request),
            _currentUser.UserId.Value);
        await _writer.AddAsync(configuration, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            BranchDisplayUniqueConstraintErrorMapper.TryMapConfiguration(ex, out var error))
        {
            return Result<BranchDisplayConfigurationResponse>.Fail(error);
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
