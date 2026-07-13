using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Query.GetServiceGlobalizationRequestById;

internal sealed class GetServiceGlobalizationRequestByIdQueryHandler
    : IQueryHandler<
        GetServiceGlobalizationRequestByIdQuery,
        ServiceGlobalizationRequestDetailsResponse>
{
    private readonly IWriteReadRepository<ServiceGlobalizationRequest>
        _requestReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;

    public GetServiceGlobalizationRequestByIdQueryHandler(
        IWriteReadRepository<ServiceGlobalizationRequest> requestReadRepository,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccessValidator)
    {
        _requestReadRepository = requestReadRepository
            ?? throw new ArgumentNullException(nameof(requestReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _branchAccessValidator = branchAccessValidator
            ?? throw new ArgumentNullException(nameof(branchAccessValidator));
    }

    public async Task<Result<ServiceGlobalizationRequestDetailsResponse>>
        Handle(
            GetServiceGlobalizationRequestByIdQuery request,
            CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "ServiceGlobalizationRequests.GetById.Unauthenticated",
                ServiceGlobalizationRequestMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        var row = await _requestReadRepository.Query()
            .AsNoTracking()
            .Where(x => x.Id == request.RequestId)
            .Select(x => new RequestDetailsProjection
            {
                RequestId = x.Id,
                RequestType = x.RequestType,
                Status = x.Status,
                BranchId = x.BranchId,
                BranchArabicName = x.Branch.ArabicName,
                BranchEnglishName = x.Branch.EnglishName,
                RootServiceId = x.RootServiceId,
                RequestedByApplicationUserId =
                    x.RequestedByApplicationUserId,
                RequestedOnUtc = x.RequestedOnUtc,
                ReviewedByApplicationUserId =
                    x.ReviewedByApplicationUserId,
                ReviewedOnUtc = x.ReviewedOnUtc,
                RejectionReason = x.RejectionReason,
                RowVersion = x.RowVersion,
                ParentService = x.RootService.ParentService == null
                    ? null
                    : new ServiceGlobalizationParentServiceResponse
                    {
                        ServiceId = x.RootService.ParentService.Id,
                        ArabicName = x.RootService.ParentService.ArabicName,
                        EnglishName = x.RootService.ParentService.EnglishName,
                        Scope = x.RootService.ParentService.Scope,
                        OwnerBranchId =
                            x.RootService.ParentService.OwnerBranchId,
                        IsActive = x.RootService.ParentService.IsActive,
                        IsDeleted = x.RootService.ParentService.IsDeleted
                    },
                Services = x.Items
                    .Select(item =>
                        new ServiceGlobalizationRequestServiceProjection
                        {
                            ServiceId = item.ServiceId,
                            ParentServiceId = item.Service.ParentServiceId,
                            ArabicName = item.Service.ArabicName,
                            EnglishName = item.Service.EnglishName,
                            Scope = item.Service.Scope,
                            OwnerBranchId = item.Service.OwnerBranchId,
                            IsActive = item.Service.IsActive,
                            IsDeleted = item.Service.IsDeleted
                        })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return Failure(
                "ServiceGlobalizationRequests.GetById.NotFound",
                ServiceGlobalizationRequestMessages.RequestNotFound,
                ErrorType.NotFound);
        }

        var branchAccess = _branchAccessValidator.EnsureCanAccessBranch(
            row.BranchId,
            "ServiceGlobalizationRequests.GetById");

        if (branchAccess.IsFailure)
        {
            return Result<ServiceGlobalizationRequestDetailsResponse>.Fail(
                branchAccess.Errors);
        }

        return Result<ServiceGlobalizationRequestDetailsResponse>.Ok(
            new ServiceGlobalizationRequestDetailsResponse
            {
                RequestId = row.RequestId,
                RequestType = row.RequestType,
                Status = row.Status,
                BranchId = row.BranchId,
                BranchArabicName = row.BranchArabicName,
                BranchEnglishName = row.BranchEnglishName,
                RootServiceId = row.RootServiceId,
                RequestedByApplicationUserId =
                    row.RequestedByApplicationUserId,
                RequestedOnUtc = row.RequestedOnUtc,
                ReviewedByApplicationUserId =
                    row.ReviewedByApplicationUserId,
                ReviewedOnUtc = row.ReviewedOnUtc,
                RejectionReason = row.RejectionReason,
                ParentService = row.ParentService,
                Services =
                    ServiceGlobalizationRequestResponseFactory
                        .BuildRequestServiceTree(row.Services),
                RowVersion = RowVersionConverter.ToBase64(row.RowVersion)
            });
    }

    private static Result<ServiceGlobalizationRequestDetailsResponse> Failure(
        string code,
        string message,
        ErrorType type)
        => Result<ServiceGlobalizationRequestDetailsResponse>.Fail(
            new Error(code, message, type));

    private sealed class RequestDetailsProjection
    {
        public int RequestId { get; init; }

        public ServiceGlobalizationRequestType RequestType { get; init; }

        public ServiceGlobalizationRequestStatus Status { get; init; }

        public int BranchId { get; init; }

        public string BranchArabicName { get; init; } = string.Empty;

        public string BranchEnglishName { get; init; } = string.Empty;

        public int RootServiceId { get; init; }

        public Guid RequestedByApplicationUserId { get; init; }

        public DateTime RequestedOnUtc { get; init; }

        public Guid? ReviewedByApplicationUserId { get; init; }

        public DateTime? ReviewedOnUtc { get; init; }

        public string? RejectionReason { get; init; }

        public ServiceGlobalizationParentServiceResponse? ParentService
        {
            get;
            init;
        }

        public IReadOnlyCollection<ServiceGlobalizationRequestServiceProjection>
            Services { get; init; } =
                Array.Empty<ServiceGlobalizationRequestServiceProjection>();

        public byte[] RowVersion { get; init; } = Array.Empty<byte>();
    }
}
