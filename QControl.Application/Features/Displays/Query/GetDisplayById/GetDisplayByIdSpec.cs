using BuildingBlock.Domain.Specification;
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
            CreatedOnUtc = x.CreatedOnUtc,
            ModifiedOnUtc = x.ModifiedOnUtc
        });
    }
}
