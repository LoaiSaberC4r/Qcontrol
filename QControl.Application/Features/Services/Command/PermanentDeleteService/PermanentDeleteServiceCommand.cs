using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Services.Command.PermanentDeleteService;

public sealed record PermanentDeleteServiceCommand
    : ICommand<PermanentDeleteServiceResponse>,
      ICacheInvalidator
{
    public int Id { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public int? OwnerBranchId { get; set; }

    public IEnumerable<string> Tags
    {
        get
        {
            var tags = new List<string>
            {
                OperationalCacheTags.Services,
                OperationalCacheTags.ServiceCentral,
                OperationalCacheTags.Service(Id),
                OperationalCacheTags.BranchServices,
                OperationalCacheTags.ServiceSchedules,
                OperationalCacheTags.ServiceWorkflows
            };

            if (OwnerBranchId.HasValue)
            {
                tags.Add(OperationalCacheTags
                    .BranchServicesForBranch(OwnerBranchId.Value));
                tags.Add(OperationalCacheTags
                    .ServiceSchedulesForBranch(OwnerBranchId.Value));
                tags.Add(OperationalCacheTags
                    .ServiceSchedule(OwnerBranchId.Value, Id));
            }

            return tags;
        }
    }
}
