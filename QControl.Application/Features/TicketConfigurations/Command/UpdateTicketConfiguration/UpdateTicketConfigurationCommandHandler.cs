using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Features.TicketConfigurations.Shared;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace QControl.Application.Features.TicketConfigurations.Command.UpdateTicketConfiguration;

internal sealed class UpdateTicketConfigurationCommandHandler
    : ICommandHandler<UpdateTicketConfigurationCommand, TicketConfigurationResponse>
{
    private const string CodePrefix = "TicketConfigurations.Update";
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<TicketPrintConfiguration> _configurations;
    private readonly IWriteRepository<TicketPrintConfiguration> _writer;
    private readonly IConcurrencyTokenManager _concurrency;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccess;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTicketConfigurationCommandHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<TicketPrintConfiguration> configurations,
        IWriteRepository<TicketPrintConfiguration> writer,
        IConcurrencyTokenManager concurrency,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccess,
        IUnitOfWork unitOfWork)
    {
        _branches = branches;
        _configurations = configurations;
        _writer = writer;
        _concurrency = concurrency;
        _currentUser = currentUser;
        _branchAccess = branchAccess;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TicketConfigurationResponse>> Handle(
        UpdateTicketConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        var guard = Guard(request.BranchId);
        if (guard.IsFailure)
        {
            return Result<TicketConfigurationResponse>.Fail(guard.Errors);
        }

        if (!await _branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken))
        {
            return Failure($"{CodePrefix}.BranchNotFound", ErrorMessage.Branch_NotFound,
                ErrorType.NotFound);
        }

        var layout = TicketPrintLayoutValidator.Validate(request.TicketWidthMm,
            request.TicketHeightMm, request.Elements, CodePrefix);
        if (layout.IsFailure)
        {
            return Result<TicketConfigurationResponse>.Fail(layout.Errors);
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Failure($"{CodePrefix}.InvalidRowVersion", ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation);
        }

        var configuration = await _configurations.FirstOrDefaultAsync(
            new GetTicketConfigurationForUpdateSpec(request.BranchId), cancellationToken);
        if (configuration is null)
        {
            return Failure($"{CodePrefix}.NotFound", TicketConfigurationFeatureMessages.NotFound,
                ErrorType.NotFound);
        }

        _concurrency.SetOriginalRowVersion(configuration, rowVersion);
        configuration.Update(request.TicketWidthMm, request.TicketHeightMm,
            TicketPrintLayoutValidator.ToSettings(request.Elements));

        // Explicitly marking the aggregate root modified guarantees SQL Server advances its
        // rowversion even when the submitted change affects child elements only.
        _writer.Update(configuration);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failure($"{CodePrefix}.ConcurrencyConflict",
                TicketConfigurationFeatureMessages.ConcurrencyConflict, ErrorType.Conflict);
        }

        return Result<TicketConfigurationResponse>.Ok(
            TicketConfigurationResponseFactory.FromEntity(configuration));
    }

    private Result Guard(int branchId)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result.Fail(new Error($"{CodePrefix}.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required, ErrorType.Unauthorized));
        }

        return _branchAccess.EnsureCanAccessBranch(branchId, CodePrefix);
    }

    private static Result<TicketConfigurationResponse> Failure(
        string code, string message, ErrorType type) =>
        Result<TicketConfigurationResponse>.Fail(new Error(code, message, type));
}
