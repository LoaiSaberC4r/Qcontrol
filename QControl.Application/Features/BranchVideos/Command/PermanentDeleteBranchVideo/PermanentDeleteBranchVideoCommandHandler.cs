using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Qcontrol.Application.Features.BranchVideos.Command.Shared;
using Qcontrol.Application.Features.BranchVideos.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Services;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchVideos.Command.PermanentDeleteBranchVideo;

internal sealed class PermanentDeleteBranchVideoCommandHandler
    : ICommandHandler<PermanentDeleteBranchVideoCommand, BranchVideoDeleteResponse>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<BranchVideo> _videos;
    private readonly IWriteRepository<BranchVideo> _videoWriter;
    private readonly IConcurrencyTokenManager _concurrency;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchVideoProcessingCoordinator _processingCoordinator;
    private readonly IBranchVideoTranscoder _transcoder;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PermanentDeleteBranchVideoCommandHandler> _logger;

    public PermanentDeleteBranchVideoCommandHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<BranchVideo> videos,
        IWriteRepository<BranchVideo> videoWriter,
        IConcurrencyTokenManager concurrency,
        ICurrentUser currentUser,
        IBranchVideoProcessingCoordinator processingCoordinator,
        IBranchVideoTranscoder transcoder,
        IMediaService mediaService,
        IUnitOfWork unitOfWork,
        ILogger<PermanentDeleteBranchVideoCommandHandler> logger)
    {
        _branches = branches;
        _videos = videos;
        _videoWriter = videoWriter;
        _concurrency = concurrency;
        _currentUser = currentUser;
        _processingCoordinator = processingCoordinator;
        _transcoder = transcoder;
        _mediaService = mediaService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BranchVideoDeleteResponse>> Handle(
        PermanentDeleteBranchVideoCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure("BranchVideos.Delete.Unauthenticated", ErrorMessage.Branch_Authentication_Required, ErrorType.Unauthorized);
        }
        if (!await _branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken))
        {
            return Failure("BranchVideos.BranchNotFound", ErrorMessage.Branch_NotFound, ErrorType.NotFound);
        }

        await using var processingLease = await _processingCoordinator.AcquireAsync(
            request.VideoId,
            cancellationToken);

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
            return Failure("BranchVideos.PermanentDelete.MustBeInactive", BranchVideoMessages.MustBeInactive, ErrorType.Conflict);
        }

        var originalPath = video.OriginalPath;
        var manifestPath = video.HlsManifestPath;
        _concurrency.SetOriginalRowVersion(video, rowVersion);
        _videoWriter.Delete(video);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failure("BranchVideos.ConcurrencyConflict", ErrorMessage.Concurrency_Conflict, ErrorType.Conflict);
        }

        try
        {
            _mediaService.Remove(originalPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Original media cleanup failed after deleting branch video {BranchVideoId}.",
                request.VideoId);
        }

        try
        {
            _transcoder.RemoveHlsOutput(originalPath, manifestPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "HLS cleanup failed after deleting branch video {BranchVideoId}.",
                request.VideoId);
        }

        return Result<BranchVideoDeleteResponse>.Ok(new BranchVideoDeleteResponse
        {
            VideoId = request.VideoId,
            BranchId = request.BranchId,
            Message = BranchVideoMessages.Deleted
        });
    }

    private static Result<BranchVideoDeleteResponse> Failure(
        string code,
        string message,
        ErrorType type) =>
        Result<BranchVideoDeleteResponse>.Fail(new Error(code, message, type));
}
