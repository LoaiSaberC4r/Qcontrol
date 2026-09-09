using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Features.TicketConfigurations.Shared;
using QControl.Domain.Entities;

namespace QControl.Application.Features.TicketConfigurations.Command.CreateTicketConfiguration;

internal sealed class CreateTicketConfigurationCommandHandler
    : ICommandHandler<CreateTicketConfigurationCommand, TicketConfigurationResponse>
{
    private const string CodePrefix = "TicketConfigurations.Create";
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<TicketPrintConfiguration> _configurations;
    private readonly IWriteRepository<TicketPrintConfiguration> _writer;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccess;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTicketConfigurationCommandHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<TicketPrintConfiguration> configurations,
        IWriteRepository<TicketPrintConfiguration> writer,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccess,
        IUnitOfWork unitOfWork)
    {
        _branches = branches;
        _configurations = configurations;
        _writer = writer;
        _currentUser = currentUser;
        _branchAccess = branchAccess;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TicketConfigurationResponse>> Handle(
        CreateTicketConfigurationCommand request,
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

        if (await _configurations.AnyAsync(x => x.BranchId == request.BranchId,
                cancellationToken))
        {
            return AlreadyExists();
        }

        var configuration = TicketPrintConfiguration.Create(request.BranchId,
            request.TicketWidthMm, request.TicketHeightMm,
            TicketPrintLayoutValidator.ToSettings(request.Elements));
        await _writer.AddAsync(configuration, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsBranchUniquenessConflict(exception))
        {
            return AlreadyExists();
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

    private static bool IsBranchUniquenessConflict(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 } sql &&
        sql.Message.Contains("UX_TicketPrintConfiguration_BranchId",
            StringComparison.OrdinalIgnoreCase);

    private static Result<TicketConfigurationResponse> AlreadyExists() =>
        Failure($"{CodePrefix}.AlreadyExists", TicketConfigurationFeatureMessages.AlreadyExists,
            ErrorType.Conflict);

    private static Result<TicketConfigurationResponse> Failure(
        string code, string message, ErrorType type) =>
        Result<TicketConfigurationResponse>.Fail(new Error(code, message, type));
}
