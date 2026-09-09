using BuildingBlock.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using QControl.Domain.Entities;

namespace QControl.Application.Features.TicketConfigurations.Command.UpdateTicketConfiguration;

internal sealed class GetTicketConfigurationForUpdateSpec
    : Specification<TicketPrintConfiguration>
{
    public GetTicketConfigurationForUpdateSpec(int branchId)
    {
        AddCriteria(x => x.BranchId == branchId);
        AddInclude(query => query.Include(x => x.Elements));
        UseTracking();
        UseSplitQuery();
    }
}
