using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Command.ApproveServiceGlobalizationRequest;

public sealed class ApproveServiceGlobalizationRequestCommand
    : ICommand<ApproveServiceGlobalizationRequestResponse>,
      ICacheInvalidator
{
    public int RequestId { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public int? BranchIdForInvalidation { get; set; }

    public IReadOnlyCollection<int> PromotedServiceIdsForInvalidation
    {
        get;
        set;
    } = Array.Empty<int>();

    public IEnumerable<string> Tags
    {
        get
        {
            var tags = new List<string>
            {
                OperationalCacheTags.Services,
                OperationalCacheTags.ServiceCentral,
                OperationalCacheTags.BranchServices,
                OperationalCacheTags.ServiceGlobalizationRequests,
                OperationalCacheTags.ServiceGlobalizationRequest(RequestId),
                OperationalCacheTags.ServiceWorkflows
            };

            if (BranchIdForInvalidation.HasValue)
            {
                tags.Add(OperationalCacheTags.BranchServicesForBranch(
                    BranchIdForInvalidation.Value));
                tags.Add(
                    OperationalCacheTags.ServiceGlobalizationRequestsForBranch(
                        BranchIdForInvalidation.Value));
            }

            tags.AddRange(PromotedServiceIdsForInvalidation
                .Select(OperationalCacheTags.Service));

            return tags;
        }
    }
}
