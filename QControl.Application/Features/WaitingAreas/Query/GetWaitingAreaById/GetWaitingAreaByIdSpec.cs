using BuildingBlock.Domain.Specification;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreaById;

internal sealed class GetWaitingAreaByIdSpec
    : Specification<WaitingArea, WaitingAreaDetailsResponse>
{
    public GetWaitingAreaByIdSpec(int id)
    {
        AddCriteria(x => x.Id == id);
        UseNoTracking();

        Select(x => new WaitingAreaDetailsResponse
        {
            Id = x.Id,
            BranchId = x.BranchId,
            BranchArabicName = x.Branch.ArabicName,
            BranchEnglishName = x.Branch.EnglishName,
            Number = x.Number,
            AudioDevice = x.AudioDevice,
            ControlDevice = x.ControlDevice,
            DescriptiveName = x.DescriptiveName,
            IsActive = x.IsActive,
            EffectiveIsActive = x.Branch.IsActive && x.IsActive,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion)
        });
    }
}
