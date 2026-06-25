using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Command.PermanentDeleteWindow;

internal sealed class GetWindowForPermanentDeleteSpec
    : Specification<Window, PermanentDeleteWindowCandidate>
{
    public GetWindowForPermanentDeleteSpec(int id)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x => x.Id == id);

        Select(x => new PermanentDeleteWindowCandidate
        {
            Id = x.Id,
            IsDeleted = x.IsDeleted
        });
    }
}
