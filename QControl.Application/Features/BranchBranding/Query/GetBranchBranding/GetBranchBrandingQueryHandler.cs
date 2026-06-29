using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchBranding.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchBranding.Query.GetBranchBranding;

internal sealed class GetBranchBrandingQueryHandler
    : IQueryHandler<GetBranchBrandingQuery, BranchBrandingResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<QControl.Domain.Entities.BranchBranding> _brandingReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetBranchBrandingQueryHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<QControl.Domain.Entities.BranchBranding> brandingReadRepository,
        ICurrentUser currentUser)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _brandingReadRepository = brandingReadRepository
            ?? throw new ArgumentNullException(nameof(brandingReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<BranchBrandingResponse>> Handle(
        GetBranchBrandingQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<BranchBrandingResponse>.Fail(new Error(
                "BranchBranding.Get.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required,
                ErrorType.Unauthorized));
        }

        var branchExists = await _branchReadRepository.AnyAsync(
            x => x.Id == request.BranchId,
            cancellationToken);

        if (!branchExists)
        {
            return Result<BranchBrandingResponse>.Fail(new Error(
                "BranchBranding.BranchNotFound",
                ErrorMessage.Branch_NotFound,
                ErrorType.NotFound));
        }

        var branding = await _brandingReadRepository.FirstOrDefaultAsync(
            new GetBranchBrandingSpec(request.BranchId),
            cancellationToken);

        return Result<BranchBrandingResponse>.Ok(
            branding ?? BranchBrandingResponseFactory.NotConfigured(
                request.BranchId));
    }
}