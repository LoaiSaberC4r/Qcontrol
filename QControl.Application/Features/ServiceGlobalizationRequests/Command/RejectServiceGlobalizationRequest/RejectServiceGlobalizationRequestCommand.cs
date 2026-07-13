using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Command.RejectServiceGlobalizationRequest;

public sealed class RejectServiceGlobalizationRequestCommand
    : ICommand<RejectServiceGlobalizationRequestResponse>,
      ICacheInvalidator
{
    public int RequestId { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public string? RejectionReason { get; init; }

    public int? BranchIdForInvalidation { get; set; }

    public IEnumerable<string> Tags
    {
        get
        {
            var tags = new List<string>
            {
                OperationalCacheTags.ServiceGlobalizationRequests,
                OperationalCacheTags.ServiceGlobalizationRequest(RequestId)
            };

            if (BranchIdForInvalidation.HasValue)
            {
                tags.Add(
                    OperationalCacheTags.ServiceGlobalizationRequestsForBranch(
                        BranchIdForInvalidation.Value));
            }

            return tags;
        }
    }
}
