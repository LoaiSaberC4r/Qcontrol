using BuildingBlock.Domain.Specification;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Query.GetDisplayById;

internal sealed class GetDisplayByIdSpec
    : Specification<Display, DisplayDetailsResponse>
{
    public GetDisplayByIdSpec(int displayId)
    {
        AddCriteria(x => x.Id == displayId);
        UseNoTracking();

        Select(x => new DisplayDetailsResponse
        {
            Id = x.Id,
            BranchId = x.BranchId,
            BranchArabicName = x.Branch.ArabicName,
            BranchEnglishName = x.Branch.EnglishName,
            Number = x.Number,
            IPAddress = x.IPAddress,
            SerialNo = x.SerialNo,
            Type = x.Type,
            IsActive = x.IsActive,
            EffectiveIsActive = x.Branch.IsActive && x.IsActive,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion),
            CreatedOnUtc = x.CreatedOnUtc,
            ModifiedOnUtc = x.ModifiedOnUtc
        });
    }
}
