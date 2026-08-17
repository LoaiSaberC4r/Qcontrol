using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchVideos.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchVideos.Query.GetBranchDisplayPlaylist;

internal sealed class GetBranchDisplayPlaylistQueryHandler
    : IQueryHandler<GetBranchDisplayPlaylistQuery, BranchDisplayPlaylistResponse>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    private readonly IWriteReadRepository<Branch> _branchRepository;
    private readonly IWriteReadRepository<BranchVideo> _videoRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cache;

    public GetBranchDisplayPlaylistQueryHandler(
        IWriteReadRepository<Branch> branchRepository,
        IWriteReadRepository<BranchVideo> videoRepository,
        ICurrentUser currentUser,
        ICacheService cache)
    {
        _branchRepository = branchRepository;
        _videoRepository = videoRepository;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<BranchDisplayPlaylistResponse>> Handle(
        GetBranchDisplayPlaylistQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<BranchDisplayPlaylistResponse>.Fail(new Error(
                "BranchVideos.Playlist.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!await _branchRepository.AnyAsync(
                x => x.Id == request.BranchId,
                cancellationToken))
        {
            return Result<BranchDisplayPlaylistResponse>.Fail(new Error(
                "BranchVideos.BranchNotFound",
                ErrorMessage.Branch_NotFound,
                ErrorType.NotFound));
        }

        var cacheKey = BranchVideoCacheTags.PlaylistKey(request.BranchId);
        var (found, cached) =
            await _cache.TryGetAsync<BranchDisplayPlaylistResponse>(
                cacheKey,
                cancellationToken);
        if (found && cached is not null)
        {
            return Result<BranchDisplayPlaylistResponse>.Ok(cached);
        }

        var videos = await _videoRepository.ListAsync(
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
