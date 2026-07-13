using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;
using QControl.Domain.Enums;

namespace QControl.Domain.Entities;

public sealed class ServiceGlobalizationRequest : AggregateRoot<int>
{
    private readonly List<ServiceGlobalizationRequestItem> _items = new();

    private ServiceGlobalizationRequest()
    {
    }

    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public int RootServiceId { get; private set; }

    public Service RootService { get; private set; } = null!;

    public ServiceGlobalizationRequestType RequestType { get; private set; }

    public ServiceGlobalizationRequestStatus Status { get; private set; }

    public Guid RequestedByApplicationUserId { get; private set; }

    public ApplicationUser RequestedByApplicationUser { get; private set; } =
        null!;

    public DateTime RequestedOnUtc { get; private set; }

    public Guid? ReviewedByApplicationUserId { get; private set; }

    public ApplicationUser? ReviewedByApplicationUser { get; private set; }

    public DateTime? ReviewedOnUtc { get; private set; }

    public string? RejectionReason { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public IReadOnlyCollection<ServiceGlobalizationRequestItem> Items =>
        _items.AsReadOnly();

    public static ServiceGlobalizationRequest Create(
        int branchId,
        int rootServiceId,
        ServiceGlobalizationRequestType requestType,
        Guid requestedByApplicationUserId,
        DateTime requestedOnUtc)
    {
        return new ServiceGlobalizationRequest
        {
            BranchId = branchId,
            RootServiceId = rootServiceId,
            RequestType = requestType,
            Status = ServiceGlobalizationRequestStatus.Pending,
            RequestedByApplicationUserId = requestedByApplicationUserId,
            RequestedOnUtc = requestedOnUtc,
            ReviewedByApplicationUserId = null,
            ReviewedOnUtc = null,
            RejectionReason = null
        };
    }

    public static ServiceGlobalizationRequest Create(
        int branchId,
        Service rootService,
        ServiceGlobalizationRequestType requestType,
        Guid requestedByApplicationUserId,
        DateTime requestedOnUtc)
    {
        var request = Create(
            branchId,
            rootServiceId: 0,
            requestType,
            requestedByApplicationUserId,
            requestedOnUtc);

        request.RootService = rootService;
        return request;
    }

    public void AddService(int serviceId)
    {
        if (_items.Any(x => x.ServiceId == serviceId))
        {
            return;
        }

        _items.Add(ServiceGlobalizationRequestItem.Create(serviceId));
    }

    public void AddService(Service service)
    {
        if (_items.Any(x => ReferenceEquals(x.Service, service)))
        {
            return;
        }

        _items.Add(ServiceGlobalizationRequestItem.Create(service));
    }

    public void Approve(
        Guid reviewedByApplicationUserId,
        DateTime reviewedOnUtc)
    {
        EnsurePending();

        Status = ServiceGlobalizationRequestStatus.Approved;
        ReviewedByApplicationUserId = reviewedByApplicationUserId;
        ReviewedOnUtc = reviewedOnUtc;
        RejectionReason = null;
    }

    public void Reject(
        Guid reviewedByApplicationUserId,
        DateTime reviewedOnUtc,
        string? rejectionReason)
    {
        EnsurePending();

        Status = ServiceGlobalizationRequestStatus.Rejected;
        ReviewedByApplicationUserId = reviewedByApplicationUserId;
        ReviewedOnUtc = reviewedOnUtc;
        RejectionReason = NormalizeOptional(rejectionReason);
    }

    private void EnsurePending()
    {
        if (Status != ServiceGlobalizationRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending requests can be reviewed.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
