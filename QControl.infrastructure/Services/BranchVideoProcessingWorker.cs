using BuildingBlock.Application.Abstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qcontrol.Application.Features.BranchVideos.Shared;
using QControl.Application.Abstraction.Services;
using QControl.Domain.Entities;
using QControl.Domain.Enums;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Services;

internal sealed class BranchVideoProcessingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBranchVideoProcessingQueue _queue;
    private readonly IBranchVideoProcessingCoordinator _coordinator;
    private readonly IBranchVideoTranscoder _transcoder;
    private readonly ICacheService _cache;
    private readonly ILogger<BranchVideoProcessingWorker> _logger;

    public BranchVideoProcessingWorker(
        IServiceScopeFactory scopeFactory,
        IBranchVideoProcessingQueue queue,
        IBranchVideoProcessingCoordinator coordinator,
        IBranchVideoTranscoder transcoder,
        ICacheService cache,
        ILogger<BranchVideoProcessingWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
        _coordinator = coordinator;
        _transcoder = transcoder;
        _cache = cache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnqueueInterruptedWorkAsync(stoppingToken);

        await foreach (var branchVideoId in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessAsync(branchVideoId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected background processing failure for branch video {BranchVideoId}.",
                    branchVideoId);
            }
        }
    }

    private async Task EnqueueInterruptedWorkAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PlatformWriteDbContext>();
        var ids = await dbContext.Set<BranchVideo>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.ProcessingStatus == BranchVideoProcessingStatus.Processing)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var id in ids)
        {
            _queue.TryEnqueue(id);
        }

        if (ids.Count > 0)
        {
            _logger.LogInformation(
                "Recovered {Count} interrupted branch video processing records.",
                ids.Count);
        }
    }

    private async Task ProcessAsync(
        int branchVideoId,
        CancellationToken cancellationToken)
    {
        await using var processingLease = await _coordinator.AcquireAsync(
            branchVideoId,
            cancellationToken);

        var work = await GetPendingWorkAsync(branchVideoId, cancellationToken);
        if (work is null)
        {
            return;
        }

        try
        {
            var manifestPath = await _transcoder.TranscodeToHlsAsync(
                work.OriginalPath,
                cancellationToken);
            if (await UpdateStatusAsync(
                    branchVideoId,
                    manifestPath,
                    failed: false,
                    cancellationToken))
            {
                await InvalidateCacheAsync(work.BranchId, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "HLS generation failed for branch video {BranchVideoId}.",
                branchVideoId);

            try
            {
                _transcoder.RemoveHlsOutput(work.OriginalPath, null);
            }
            catch (Exception cleanupException)
            {
                _logger.LogWarning(
                    cleanupException,
                    "Incomplete HLS cleanup failed for branch video {BranchVideoId}.",
                    branchVideoId);
            }

            if (await UpdateStatusAsync(
                    branchVideoId,
                    manifestPath: null,
                    failed: true,
                    cancellationToken))
            {
                await InvalidateCacheAsync(work.BranchId, cancellationToken);
            }
        }
    }

    private async Task<PendingWork?> GetPendingWorkAsync(
        int branchVideoId,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PlatformWriteDbContext>();
        return await dbContext.Set<BranchVideo>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == branchVideoId &&
                        x.ProcessingStatus == BranchVideoProcessingStatus.Processing)
            .Select(x => new PendingWork(x.BranchId, x.OriginalPath))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<bool> UpdateStatusAsync(
        int branchVideoId,
        string? manifestPath,
        bool failed,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PlatformWriteDbContext>();
            var video = await dbContext.Set<BranchVideo>()
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.Id == branchVideoId, cancellationToken);
            if (video is null ||
                video.ProcessingStatus != BranchVideoProcessingStatus.Processing)
            {
                return false;
            }

            if (failed)
            {
                video.MarkFailed();
            }
            else
            {
                video.MarkReady(manifestPath!);
            }

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (attempt == 3)
                {
                    _logger.LogWarning(
                        "Processing status update exhausted concurrency retries for branch video {BranchVideoId}.",
                        branchVideoId);
                    return false;
                }

                _logger.LogInformation(
                    "Retrying processing status update after concurrency for branch video {BranchVideoId}.",
                    branchVideoId);
            }
        }

        _logger.LogWarning(
            "Processing output exists but status could not be updated for branch video {BranchVideoId}.",
            branchVideoId);
        return false;
    }

    private Task InvalidateCacheAsync(
        int branchId,
        CancellationToken cancellationToken) =>
        _cache.InvalidateByTagsAsync(
            BranchVideoCacheTags.ForBranch(branchId),
            cancellationToken);

    private sealed record PendingWork(int BranchId, string OriginalPath);
}
