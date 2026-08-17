using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchVideos.Command.Shared;
using Qcontrol.Application.Features.BranchVideos.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchVideos.Command.ReactivateBranchVideo;

internal sealed class ReactivateBranchVideoCommandHandler
    : ICommandHandler<ReactivateBranchVideoCommand, BranchVideoStateResponse>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<BranchVideo> _videos;
    private readonly IWriteRepository<BranchVideo> _videoWriter;
    private readonly IConcurrencyTokenManager _concurrency;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateBranchVideoCommandHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<BranchVideo> videos,
        IWriteRepository<BranchVideo> videoWriter,
        IConcurrencyTokenManager concurrency,
        ICurrentUser currentUser,
        IDateTimeProvider clock,
        IUnitOfWork unitOfWork)
    {
        _branches = branches;
        _videos = videos;
        _videoWriter = videoWriter;
        _concurrency = concurrency;
        _currentUser = currentUser;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BranchVideoStateResponse>> Handle(
        ReactivateBranchVideoCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure("BranchVideos.Reactivate.Unauthenticated", ErrorMessage.Branch_Authentication_Required, ErrorType.Unauthorized);
        }
        if (!await _branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken))
        {
            return Failure("BranchVideos.BranchNotFound", ErrorMessage.Branch_NotFound, ErrorType.NotFound);
        }

        var video = await _videos.FirstOrDefaultAsync(
            new GetBranchVideoForMutationSpec(request.VideoId), cancellationToken);
        if (video is null)
        {
            return Failure("BranchVideos.NotFound", BranchVideoMessages.NotFound, ErrorType.NotFound);
        }
        if (video.BranchId != request.BranchId)
        {
            return Failure("BranchVideos.DoesNotBelongToBranch", BranchVideoMessages.OwnershipMismatch, ErrorType.Conflict);
        }
        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Failure("BranchVideos.InvalidRowVersion", ErrorMessage.RowVersion_Invalid, ErrorType.Validation);
        }
        if (video.IsActive)
        {
            return Failure("BranchVideos.AlreadyActive", BranchVideoMessages.AlreadyActive, ErrorType.Conflict);
        }

        _concurrency.SetOriginalRowVersion(video, rowVersion);
        video.Reactivate(_clock.UtcNow, _currentUser.UserId.Value);
        _videoWriter.Update(video);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failure("BranchVideos.ConcurrencyConflict", ErrorMessage.Concurrency_Conflict, ErrorType.Conflict);
        }

        return Result<BranchVideoStateResponse>.Ok(
            BranchVideoResponseFactory.StateFromEntity(video, BranchVideoMessages.Reactivated));
    }

    private static Result<BranchVideoStateResponse> Failure(string code, string message, ErrorType type) =>
        Result<BranchVideoStateResponse>.Fail(new Error(code, message, type));
}
