using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Segments.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Segments.Command.CreateGlobalSegment;

internal sealed class CreateGlobalSegmentCommandHandler
    : ICommandHandler<CreateGlobalSegmentCommand, SegmentResponse>
{
    private readonly IWriteReadRepository<Segment> _segmentRead;
    private readonly IWriteRepository<Segment> _segmentWrite;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _branchContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGlobalSegmentCommandHandler(
        IWriteReadRepository<Segment> segmentRead,
        IWriteRepository<Segment> segmentWrite,
        ICurrentUser currentUser,
        ICurrentBranchContext branchContext,
        IUnitOfWork unitOfWork)
    {
        _segmentRead = segmentRead;
        _segmentWrite = segmentWrite;
        _currentUser = currentUser;
        _branchContext = branchContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SegmentResponse>> Handle(
        CreateGlobalSegmentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "Segments.AuthenticationRequired",
                SegmentFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        if (!_branchContext.IsSystemLevelActor)
        {
            return Failure(
                "Segments.TechnicalAdminRequired",
                SegmentFeatureMessages.TechnicalAdminRequired,
                ErrorType.Security);
        }

        var count = await _segmentRead.Query().CountAsync(cancellationToken);
        if (request.Priority > count)
        {
            return Failure(
                "Segments.PriorityExceedsSegmentCount",
                SegmentFeatureMessages.PriorityExceedsCount,
                ErrorType.Validation);
        }

        var segment = Segment.CreateGlobal(
            request.ArabicName,
            request.EnglishName,
            request.Priority,
            _currentUser.UserId.Value);
        await _segmentWrite.AddAsync(segment, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Failure(
                "Segments.PersistenceConflict",
                SegmentFeatureMessages.ConcurrencyConflict,
                ErrorType.Conflict);
        }

        return Result<SegmentResponse>.Ok(ToResponse(segment));
    }

    internal static SegmentResponse ToResponse(Segment segment) => new()
    {
        Id = segment.Id,
        ArabicName = segment.ArabicName,
        EnglishName = segment.EnglishName,
        Priority = segment.Priority,
        Scope = segment.Scope,
        OwnerBranchId = segment.OwnerBranchId,
        IsSystemDefault = segment.IsSystemDefault,
        RowVersion = RowVersionConverter.ToBase64(segment.RowVersion),
        CreatedOnUtc = segment.CreatedOnUtc,
        ModifiedOnUtc = segment.ModifiedOnUtc,
        Message = SegmentFeatureMessages.CreateSuccess
    };

    private static Result<SegmentResponse> Failure(
        string code,
        string message,
        ErrorType type) =>
        Result<SegmentResponse>.Fail(new Error(code, message, type));
}
