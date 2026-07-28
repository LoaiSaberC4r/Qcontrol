using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Segments.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Segments.Command.CreateBranchSegment;

internal sealed class CreateBranchSegmentCommandHandler
    : ICommandHandler<CreateBranchSegmentCommand, CreateBranchSegmentResponse>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<Segment> _segmentRead;
    private readonly IWriteRepository<Segment> _segmentWrite;
    private readonly IWriteRepository<SegmentGlobalizationRequest>
        _requestWrite;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _branchContext;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchSegmentCommandHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<Segment> segmentRead,
        IWriteRepository<Segment> segmentWrite,
        IWriteRepository<SegmentGlobalizationRequest> requestWrite,
        ICurrentUser currentUser,
        ICurrentBranchContext branchContext,
        IDateTimeProvider clock,
        IUnitOfWork unitOfWork)
    {
        _branches = branches;
        _segmentRead = segmentRead;
        _segmentWrite = segmentWrite;
        _requestWrite = requestWrite;
        _currentUser = currentUser;
        _branchContext = branchContext;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateBranchSegmentResponse>> Handle(
        CreateBranchSegmentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "Segments.AuthenticationRequired",
                SegmentFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        if (!_branchContext.IsBranchActor)
        {
            return Failure(
                "Segments.BranchAdminRequired",
                SegmentFeatureMessages.BranchAdminRequired,
                ErrorType.Security);
        }

        if (_branchContext.ActiveBranchId != request.BranchId)
        {
            return Failure(
                "Segments.BranchAccessForbidden",
                SegmentFeatureMessages.BranchAccessForbidden,
                ErrorType.Security);
        }

        var branchExists = await _branches.Query()
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.BranchId, cancellationToken);
        if (!branchExists)
        {
            return Failure(
                "Segments.BranchNotFound",
                SegmentFeatureMessages.BranchNotFound,
                ErrorType.NotFound);
        }

        var count = await _segmentRead.Query().CountAsync(cancellationToken);
        if (request.Priority > count)
        {
            return Failure(
                "Segments.PriorityExceedsSegmentCount",
                SegmentFeatureMessages.PriorityExceedsCount,
                ErrorType.Validation);
        }

        var userId = _currentUser.UserId.Value;
        var segment = Segment.CreateBranchScoped(
            request.BranchId,
            request.ArabicName,
            request.EnglishName,
            request.Priority,
            userId);
        var globalizationRequest = SegmentGlobalizationRequest.Create(
            request.BranchId,
            segment,
            userId,
            _clock.UtcNow);

        await _segmentWrite.AddAsync(segment, cancellationToken);
        await _requestWrite.AddAsync(
            globalizationRequest,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.ToString().Contains(
                "UX_SegmentGlobalizationRequest_SegmentId_Pending",
                StringComparison.OrdinalIgnoreCase))
        {
            return Failure(
                "SegmentGlobalizationRequests.AlreadyPending",
                SegmentFeatureMessages.PendingRequestExists,
                ErrorType.Conflict);
        }
        catch (DbUpdateException)
        {
            return Failure(
                "Segments.PersistenceConflict",
                SegmentFeatureMessages.ConcurrencyConflict,
                ErrorType.Conflict);
        }

        return Result<CreateBranchSegmentResponse>.Ok(
            new CreateBranchSegmentResponse
            {
                Id = segment.Id,
                ArabicName = segment.ArabicName,
                EnglishName = segment.EnglishName,
                Priority = segment.Priority,
                Scope = segment.Scope,
                OwnerBranchId = segment.OwnerBranchId,
                IsSystemDefault = false,
                RowVersion = RowVersionConverter.ToBase64(
                    segment.RowVersion),
                CreatedOnUtc = segment.CreatedOnUtc,
                GlobalizationRequestId = globalizationRequest.Id,
                GlobalizationRequestStatus = globalizationRequest.Status,
                Message = SegmentFeatureMessages.CreateSuccess
            });
    }

    private static Result<CreateBranchSegmentResponse> Failure(
        string code,
        string message,
        ErrorType type) =>
        Result<CreateBranchSegmentResponse>.Fail(
            new Error(code, message, type));
}
