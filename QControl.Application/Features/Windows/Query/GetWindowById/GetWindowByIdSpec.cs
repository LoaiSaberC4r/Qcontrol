using BuildingBlock.Domain.Specification;
using QControl.Application.Shared.Operational;
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
            BranchId = x.BranchId,
            WaitingAreaId = x.WaitingAreaId,
            WaitingAreaNumber = x.WaitingArea.Number,
            WaitingAreaDescriptiveName = x.WaitingArea.DescriptiveName,
            Number = x.Number,
            DescriptiveName = x.DescriptiveName,
            IPAddress = x.IPAddress,
            EnableTicketBooking = x.EnableTicketBooking,
            EnableDirectCall = x.EnableDirectCall,
            IsActive = x.IsActive,
            EffectiveIsActive =
                x.WaitingArea.Branch.IsActive &&
                x.WaitingArea.IsActive &&
                x.IsActive,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion),
            CreatedOnUtc = x.CreatedOnUtc,
            ModifiedOnUtc = x.ModifiedOnUtc
        });
    }
}
