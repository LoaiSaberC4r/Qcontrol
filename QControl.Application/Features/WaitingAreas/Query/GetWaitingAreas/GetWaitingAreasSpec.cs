using BuildingBlock.Domain.Specification;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreas;

internal sealed class GetWaitingAreasSpec
    : Specification<WaitingArea, WaitingAreaListItemResponse>
{
    public GetWaitingAreasSpec(GetWaitingAreasQuery query)
    {
        if (query.BranchId.HasValue)
        {
            var branchId = query.BranchId.Value;
            AddCriteria(x => x.BranchId == branchId);
        }

        if (query.IsActive.HasValue)
        {
            var isActive = query.IsActive.Value;
            AddCriteria(x => x.IsActive == isActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchText = query.Search.Trim();
            var numberSearch = int.TryParse(
                searchText,
                out var parsedNumber);

            if (StatusSearchTerm.TryParse(searchText, out var status))
            {
                AddCriteria(x => x.IsActive == status);
            }
            else
            {
                AddCriteria(x =>
                    (x.DescriptiveName != null &&
                        x.DescriptiveName.Contains(searchText)) ||
                    (x.AudioDevice != null &&
                        x.AudioDevice.Contains(searchText)) ||
                    (x.ControlDevice != null &&
                        x.ControlDevice.Contains(searchText)) ||
                    (numberSearch && x.Number == parsedNumber));
            }
        }

        AddOrderBy(x => x.BranchId);
        AddOrderBy(x => x.Number);
        AddOrderBy(x => x.Id);

        EnableTotalCount();

        ApplyPaging(
            query.PageNumber,
            query.PageSize);

        UseNoTracking();

        Select(x => new WaitingAreaListItemResponse
        {
            Id = x.Id,
            BranchId = x.BranchId,
            BranchArabicName = x.Branch.ArabicName,
            BranchEnglishName = x.Branch.EnglishName,
            Number = x.Number,
            DescriptiveName = x.DescriptiveName,
            AudioDevice = x.AudioDevice,
            ControlDevice = x.ControlDevice,
            IsActive = x.IsActive,
            EffectiveIsActive = x.Branch.IsActive && x.IsActive,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion),
            WindowsCount = x.Windows.Count
        });
    }
}
