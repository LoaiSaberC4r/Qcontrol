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

namespace Qcontrol.Application.Features.BranchVideos.Command.DeactivateBranchVideo;

internal sealed class DeactivateBranchVideoCommandHandler
    : ICommandHandler<DeactivateBranchVideoCommand, BranchVideoStateResponse>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<BranchVideo> _videos;
    private readonly IWriteRepository<BranchVideo> _videoWriter;
    private readonly IConcurrencyTokenManager _concurrency;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateBranchVideoCommandHandler(
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
        DeactivateBranchVideoCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await LoadAsync(request.BranchId, request.VideoId, cancellationToken);
        if (loaded.IsFailure)
        {
            return Result<BranchVideoStateResponse>.Fail(loaded.Errors);
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Failure("BranchVideos.InvalidRowVersion", ErrorMessage.RowVersion_Invalid, ErrorType.Validation);
        }

        var video = loaded.Value;
        if (!video.IsActive)
        {
            return Failure("BranchVideos.AlreadyInactive", BranchVideoMessages.AlreadyInactive, ErrorType.Conflict);
        }

        _concurrency.SetOriginalRowVersion(video, rowVersion);
        video.Deactivate(_clock.UtcNow, _currentUser.UserId!.Value);
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
            BranchVideoResponseFactory.StateFromEntity(video, BranchVideoMessages.Deactivated));
    }

    private async Task<Result<BranchVideo>> LoadAsync(int branchId, int videoId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<BranchVideo>.Fail(new Error(
                "BranchVideos.Deactivate.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required,
                ErrorType.Unauthorized));
        }
        if (!await _branches.AnyAsync(x => x.Id == branchId, ct))
        {
            return Result<BranchVideo>.Fail(new Error(
                "BranchVideos.BranchNotFound", ErrorMessage.Branch_NotFound, ErrorType.NotFound));
        }

        var video = await _videos.FirstOrDefaultAsync(new GetBranchVideoForMutationSpec(videoId), ct);
        if (video is null)
        {
            return Result<BranchVideo>.Fail(new Error(
                "BranchVideos.NotFound", BranchVideoMessages.NotFound, ErrorType.NotFound));
        }
        if (video.BranchId != branchId)
        {
            return Result<BranchVideo>.Fail(new Error(
                "BranchVideos.DoesNotBelongToBranch", BranchVideoMessages.OwnershipMismatch, ErrorType.Conflict));
        }
        return Result<BranchVideo>.Ok(video);
    }

    private static Result<BranchVideoStateResponse> Failure(string code, string message, ErrorType type) =>
        Result<BranchVideoStateResponse>.Fail(new Error(code, message, type));
}
