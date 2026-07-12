using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Query.GetBranchesPagination;

internal sealed class GetBranchesPaginationQueryHandler
    : IQueryHandler<
        GetBranchesPaginationQuery,
        Pagination<BranchPaginationItemResponse>>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetBranchesPaginationQueryHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        ICurrentUser currentUser)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<Pagination<BranchPaginationItemResponse>>> Handle(
        GetBranchesPaginationQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<BranchPaginationItemResponse>>.Fail(
                    new Error(
                        Code: "Branches.Pagination.Unauthenticated",
                        Message: ErrorMessage.Branch_Authentication_Required,
                        Type: ErrorType.Unauthorized));
            }

            request.SearchText ??= string.Empty;

            var specification =
                new GetBranchesPaginationSpec(request);

            var (items, totalCount) =
                await _branchReadRepository.ListWithCountAsync(
                    specification,
                    cancellationToken);

            var response = new Pagination<BranchPaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: items);

            return Result<Pagination<BranchPaginationItemResponse>>.Ok(
                response);
        }catch (Exception ex)
        {
 return Result<Pagination<BranchPaginationItemResponse>>.Fail(
                new Error(
                    Code: "Branches.Pagination.Exception",
                    Message: ex.Message,
                    Type: ErrorType.Unknown));
        }
    }
}