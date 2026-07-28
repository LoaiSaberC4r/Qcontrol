using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Segments.Command.CreateGlobalSegment;
using Qcontrol.Application.Features.Segments.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.Segments.Command.UpdateSegment;

internal sealed class UpdateSegmentCommandHandler
    : ICommandHandler<UpdateSegmentCommand, SegmentResponse>
{
    private readonly IWriteReadRepository<Segment> _segments;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _branchContext;
    private readonly IConcurrencyTokenManager _concurrency;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSegmentCommandHandler(
        IWriteReadRepository<Segment> segments,
        ICurrentUser currentUser,
        ICurrentBranchContext branchContext,
        IConcurrencyTokenManager concurrency,
        IDateTimeProvider clock,
        IUnitOfWork unitOfWork)
    {
        _segments = segments;
        _currentUser = currentUser;
        _branchContext = branchContext;
        _concurrency = concurrency;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SegmentResponse>> Handle(
        UpdateSegmentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "Segments.AuthenticationRequired",
                SegmentFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        if (!RowVersionConverter.TryDecode(
            request.RowVersion,
            out var rowVersion))
        {
            return Failure(
                "Segments.RowVersionInvalid",
                SegmentFeatureMessages.InvalidRowVersion,
                ErrorType.Validation);
        }

        var segment = await _segments.Query()
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.SegmentId,
                cancellationToken);
        if (segment is null)
        {
            return Failure(
                "Segments.NotFound",
                SegmentFeatureMessages.NotFound,
                ErrorType.NotFound);
        }

        var accessError = ValidateAccess(segment);
        if (accessError is not null)
        {
            return Result<SegmentResponse>.Fail(accessError);
        }

        var count = await _segments.Query().CountAsync(cancellationToken);
        if (request.Priority > count)
        {
            return Failure(
                "Segments.PriorityExceedsSegmentCount",
                SegmentFeatureMessages.PriorityExceedsCount,
                ErrorType.Validation);
        }

        if (segment.IsSystemDefault && request.Priority != 0)
        {
            return Failure(
                "Segments.DefaultPriorityCannotChange",
                SegmentFeatureMessages.DefaultPriorityCannotChange,
                ErrorType.Validation);
        }

        _concurrency.SetOriginalRowVersion(segment, rowVersion);
        segment.UpdateDefinition(
            request.ArabicName,
            request.EnglishName,
            request.Priority,
            _currentUser.UserId.Value,
            _clock.UtcNow);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failure(
                "Segments.ConcurrencyConflict",
                SegmentFeatureMessages.ConcurrencyConflict,
                ErrorType.Conflict);
        }
        catch (DbUpdateException)
        {
            return Failure(
                "Segments.PersistenceConflict",
                SegmentFeatureMessages.ConcurrencyConflict,
                ErrorType.Conflict);
        }

        var response =
            CreateGlobalSegmentCommandHandler.ToResponse(segment);
        return Result<SegmentResponse>.Ok(new SegmentResponse
        {
            Id = response.Id,
            ArabicName = response.ArabicName,
            EnglishName = response.EnglishName,
            Priority = response.Priority,
            Scope = response.Scope,
            OwnerBranchId = response.OwnerBranchId,
            IsSystemDefault = response.IsSystemDefault,
            RowVersion = response.RowVersion,
            CreatedOnUtc = response.CreatedOnUtc,
            ModifiedOnUtc = response.ModifiedOnUtc,
            Message = SegmentFeatureMessages.UpdateSuccess
        });
    }

    private Error? ValidateAccess(Segment segment)
    {
        if (_branchContext.IsSystemLevelActor)
        {
            return null;
        }

        if (!_branchContext.IsBranchActor ||
            !_branchContext.ActiveBranchId.HasValue)
        {
            return new Error(
                "Segments.BranchAdminRequired",
                SegmentFeatureMessages.BranchAdminRequired,
                ErrorType.Security);
        }

        if (segment.IsSystemDefault)
        {
            return new Error(
                "Segments.DefaultCanOnlyBeUpdatedByTechnicalAdmin",
                SegmentFeatureMessages.DefaultOnlyTechnicalAdmin,
                ErrorType.Security);
        }

        if (segment.Scope == SegmentScope.Global)
        {
            return new Error(
                "Segments.GlobalSegmentForbiddenForBranchAdmin",
                SegmentFeatureMessages.GlobalForbidden,
                ErrorType.Security);
        }

        return segment.OwnerBranchId == _branchContext.ActiveBranchId
            ? null
            : new Error(
                "Segments.ForeignBranchSegmentForbidden",
                SegmentFeatureMessages.ForeignBranchForbidden,
                ErrorType.Security);
    }

    private static Result<SegmentResponse> Failure(
        string code,
        string message,
        ErrorType type) =>
        Result<SegmentResponse>.Fail(new Error(code, message, type));
}
