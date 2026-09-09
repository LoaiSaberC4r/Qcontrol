using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace QControl.Application.Features.TicketConfigurations.Shared;

internal static class TicketConfigurationResponseFactory
{
    public static TicketConfigurationResponse FromEntity(TicketPrintConfiguration entity) =>
        new()
        {
            Id = entity.Id,
            BranchId = entity.BranchId,
            TicketWidthMm = entity.TicketWidthMm,
            TicketHeightMm = entity.TicketHeightMm,
            RowVersion = RowVersionConverter.ToBase64(entity.RowVersion),
            Elements = entity.Elements.OrderBy(x => x.ElementType).Select(x =>
                new TicketPrintElementResponse(x.ElementType, x.IsVisible, x.XMm, x.YMm,
                    x.WidthMm, x.HeightMm, x.FontSizePt, x.FontWeight, x.TextAlign,
                    x.Language)).ToArray()
        };
}
