using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Qcontrol.Application.Features.BranchVideos.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Services;
using QControl.Application.Shared.Validation;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchVideos.Command.AddBranchVideo;

internal sealed class AddBranchVideoCommandHandler
    : ICommandHandler<AddBranchVideoCommand, BranchVideoResponse>
{
    private readonly IWriteReadRepository<Branch> _branchRepository;
    private readonly IWriteReadRepository<BranchVideo> _videoReadRepository;
    private readonly IWriteRepository<BranchVideo> _videoWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IMediaService _mediaService;
    private readonly IBranchVideoProcessingQueue _processingQueue;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddBranchVideoCommandHandler> _logger;

    public AddBranchVideoCommandHandler(
        IWriteReadRepository<Branch> branchRepository,
        IWriteReadRepository<BranchVideo> videoReadRepository,
        IWriteRepository<BranchVideo> videoWriteRepository,
        ICurrentUser currentUser,
        IMediaService mediaService,
        IBranchVideoProcessingQueue processingQueue,
        IUnitOfWork unitOfWork,
        ILogger<AddBranchVideoCommandHandler> logger)
    {
        _branchRepository = branchRepository;
        _videoReadRepository = videoReadRepository;
        _videoWriteRepository = videoWriteRepository;
        _currentUser = currentUser;
        _mediaService = mediaService;
        _processingQueue = processingQueue;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BranchVideoResponse>> Handle(
        AddBranchVideoCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "BranchVideos.Add.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required,
                ErrorType.Unauthorized);
        }

        var validationError = BranchVideoFileValidator.Validate(
            request.Video,
            BranchVideoMessages.VideoRequired,
            BranchVideoMessages.InvalidVideoType);
        if (validationError is not null)
        {
            return Result<BranchVideoResponse>.Fail(validationError);
        }

        if (request.DisplayOrder <= 0)
        {
            return Failure(
                "BranchVideos.InvalidDisplayOrder",
                BranchVideoMessages.InvalidDisplayOrder,
                ErrorType.Validation);
        }

        var branchExists = await _branchRepository.AnyAsync(
            x => x.Id == request.BranchId,
            cancellationToken);
        if (!branchExists)
        {
            return Failure(
                "BranchVideos.BranchNotFound",
                ErrorMessage.Branch_NotFound,
                ErrorType.NotFound);
        }

        var orderExists = await _videoReadRepository.AnyAsync(
            x => x.BranchId == request.BranchId &&
                 x.DisplayOrder == request.DisplayOrder,
            cancellationToken);
        if (orderExists)
        {
            return Failure(
                "BranchVideos.OrderConflict",
                BranchVideoMessages.OrderConflict,
                ErrorType.Conflict);
        }

        var mediaKey = Guid.NewGuid().ToString("N");
        string originalPath;
        try
        {
            originalPath = await _mediaService.SaveVideoAsync(
                request.Video!,
                $"Branches/{request.BranchId}/Videos/{mediaKey}/original");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Branch video original media save failed for branch {BranchId}.",
                request.BranchId);
            return Failure(
                "BranchVideos.MediaSaveFailed",
                BranchVideoMessages.MediaSaveFailed,
                ErrorType.Infrastructure);
        }

        var video = BranchVideo.Create(
            request.BranchId,
            NormalizeOriginalFileName(request.Video!.FileName),
            originalPath,
            request.DisplayOrder,
            _currentUser.UserId.Value);

        await _videoWriteRepository.AddAsync(video, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (BranchVideoUniqueConstraintErrorMapper.TryMapOrderConflict(
                ex,
                out var error))
        {
            SafeRemoveOriginal(originalPath);
            return Result<BranchVideoResponse>.Fail(error);
        }
        catch (Exception ex)
        {
            SafeRemoveOriginal(originalPath);
            _logger.LogError(
                ex,
                "Branch video persistence failed for branch {BranchId}.",
                request.BranchId);
            return Failure(
                "BranchVideos.PersistenceFailed",
                BranchVideoMessages.PersistenceFailed,
                ErrorType.Infrastructure);
        }

        if (!_processingQueue.TryEnqueue(video.Id))
        {
            _logger.LogWarning(
                "Branch video {BranchVideoId} was persisted but could not be queued; startup recovery will retry it.",
                video.Id);
        }

        return Result<BranchVideoResponse>.Ok(
            BranchVideoResponseFactory.FromEntity(video));
    }

    private void SafeRemoveOriginal(string path)
    {
        try
        {
            _mediaService.Remove(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Compensating branch video media cleanup failed for {Path}.",
                path);
        }
    }

    private static string NormalizeOriginalFileName(string fileName)
    {
        var normalized = fileName.Replace('\\', '/');
        normalized = normalized[(normalized.LastIndexOf('/') + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            normalized = "video";
        }

        return normalized.Length <= 260 ? normalized : normalized[..260];
    }

    private static Result<BranchVideoResponse> Failure(
        string code,
        string message,
        ErrorType type) =>
        Result<BranchVideoResponse>.Fail(new Error(code, message, type));
}
