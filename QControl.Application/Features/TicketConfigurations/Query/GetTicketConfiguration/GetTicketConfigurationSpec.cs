using BuildingBlock.Domain.Specification;
using QControl.Application.Features.TicketConfigurations.Shared;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace QControl.Application.Features.TicketConfigurations.Query.GetTicketConfiguration;

internal sealed class GetTicketConfigurationSpec
    : Specification<TicketPrintConfiguration, TicketConfigurationResponse>
{
    public GetTicketConfigurationSpec(int branchId)
    {
        AddCriteria(x => x.BranchId == branchId);
        UseNoTracking();
        Select(x => new TicketConfigurationResponse
        {
            Id = x.Id,
            BranchId = x.BranchId,
            TicketWidthMm = x.TicketWidthMm,
            TicketHeightMm = x.TicketHeightMm,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion),
            Elements = x.Elements.OrderBy(element => element.ElementType)
                .Select(element => new TicketPrintElementResponse(
                    element.ElementType, element.IsVisible, element.XMm, element.YMm,
                    element.WidthMm, element.HeightMm, element.FontSizePt,
                    element.FontWeight, element.TextAlign, element.Language)).ToArray()
        });
    }
}
