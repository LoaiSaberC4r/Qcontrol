using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchVideos.Query.GetBranchDisplayPlaylist;
using Qcontrol.Application.Features.BranchVideos.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchVideos.Query.GetBranchDisplayRuntimePlaylist;

internal sealed class GetBranchDisplayRuntimePlaylistQueryHandler
    : IQueryHandler<GetBranchDisplayRuntimePlaylistQuery, BranchDisplayPlaylistResponse>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<BranchVideo> _videos;
    private readonly ICacheService _cache;

    public GetBranchDisplayRuntimePlaylistQueryHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<BranchVideo> videos,
        ICacheService cache)
    {
        _branches = branches;
        _videos = videos;
        _cache = cache;
    }

    public async Task<Result<BranchDisplayPlaylistResponse>> Handle(
        GetBranchDisplayRuntimePlaylistQuery request,
        CancellationToken cancellationToken)
    {
        if (!await _branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken))
        {
            return Result<BranchDisplayPlaylistResponse>.Fail(new Error(
                "BranchVideos.RuntimePlaylist.BranchNotFound",
                ErrorMessage.Branch_NotFound,
                ErrorType.NotFound));
        }

        var cacheKey = BranchVideoCacheTags.PlaylistKey(request.BranchId);
        var (found, cached) = await _cache.TryGetAsync<BranchDisplayPlaylistResponse>(cacheKey, cancellationToken);
        if (found && cached is not null)
        {
            return Result<BranchDisplayPlaylistResponse>.Ok(cached);
        }

        var videos = await _videos.ListAsync(
            new GetBranchDisplayPlaylistSpec(request.BranchId),
            cancellationToken);
        var response = new BranchDisplayPlaylistResponse
        {
            BranchId = request.BranchId,
            Videos = videos
        };
        await _cache.SetAsync(
            cacheKey,
            response,
            CacheDuration,
            BranchVideoCacheTags.ForBranch(request.BranchId),
            cancellationToken);
        return Result<BranchDisplayPlaylistResponse>.Ok(response);
    }
}
