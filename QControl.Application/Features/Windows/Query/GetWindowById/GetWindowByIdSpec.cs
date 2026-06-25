using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Query.GetWindowById;

internal sealed class GetWindowByIdSpec
    : Specification<Window, WindowDetailsResponse>
{
    public GetWindowByIdSpec(int id)
    {
        AddCriteria(x => x.Id == id);
        UseNoTracking();

        Select(x => new WindowDetailsResponse
        {
            Id = x.Id,
            WaitingAreaId = x.WaitingAreaId,
            WaitingAreaNumber = x.WaitingArea.Number,
            WaitingAreaDescriptiveName = x.WaitingArea.DescriptiveName,
            Number = x.Number,
            DescriptiveName = x.DescriptiveName,
            IPAddress = x.IPAddress,
            EnableTicketBooking = x.EnableTicketBooking,
            EnableDirectCall = x.EnableDirectCall,
            CreatedOnUtc = x.CreatedOnUtc,
            ModifiedOnUtc = x.ModifiedOnUtc
        });
    }
}
