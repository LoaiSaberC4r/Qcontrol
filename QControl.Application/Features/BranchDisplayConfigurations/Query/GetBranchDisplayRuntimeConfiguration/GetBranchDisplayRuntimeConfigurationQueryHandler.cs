using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Query.GetBranchDisplayRuntimeConfiguration;

internal sealed class GetBranchDisplayRuntimeConfigurationQueryHandler
    : IQueryHandler<GetBranchDisplayRuntimeConfigurationQuery, BranchDisplayRuntimeConfigurationResponse>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<BranchDisplayMessage> _messages;
    private readonly ICacheService _cache;

    public GetBranchDisplayRuntimeConfigurationQueryHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<BranchDisplayMessage> messages,
        ICacheService cache)
    {
        _branches = branches;
        _messages = messages;
        _cache = cache;
    }

    public async Task<Result<BranchDisplayRuntimeConfigurationResponse>> Handle(
        GetBranchDisplayRuntimeConfigurationQuery request,
        CancellationToken cancellationToken)
    {
        var header = await _branches.FirstOrDefaultAsync(
            new GetBranchDisplayRuntimeHeaderSpec(request.BranchId),
            cancellationToken);
        if (header is null)
        {
            return Result<BranchDisplayRuntimeConfigurationResponse>.Fail(new Error(
                "BranchDisplayRuntime.BranchNotFound",
                ErrorMessage.Branch_NotFound,
                ErrorType.NotFound));
        }

        var cacheKey = BranchDisplayCacheTags.RuntimeConfigurationKey(request.BranchId);
        var (found, cached) = await _cache.TryGetAsync<BranchDisplayRuntimeConfigurationResponse>(
            cacheKey,
            cancellationToken);
        if (found && cached is not null)
        {
            return Result<BranchDisplayRuntimeConfigurationResponse>.Ok(cached);
        }

        var messages = await _messages.ListAsync(
            new GetActiveBranchDisplayMessagesSpec(request.BranchId),
            cancellationToken);
        var response = new BranchDisplayRuntimeConfigurationResponse
        {
            BranchId = header.BranchId,
            Branding = header.Branding,
            DisplayConfiguration = header.DisplayConfiguration,
            Messages = messages
        };

        await _cache.SetAsync(
            cacheKey,
            response,
            CacheDuration,
            BranchDisplayCacheTags.ForBranch(request.BranchId),
            cancellationToken);
        return Result<BranchDisplayRuntimeConfigurationResponse>.Ok(response);
    }
}
