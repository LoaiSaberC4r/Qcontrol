using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.DisplayWindows.Command.UnassignWindowFromDisplay;

internal sealed class GetDisplayWindowLinkSpec
    : Specification<DisplayWindow>
{
    public GetDisplayWindowLinkSpec(
        int displayId,
        int windowId)
    {
        AddCriteria(x =>
            x.DisplayId == displayId &&
            x.WindowId == windowId);
    }
}
