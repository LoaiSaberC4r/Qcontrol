using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.WaitingAreas.Command.CreateWaitingArea;

public sealed record CreateWaitingAreaCommand
    : ICommand<CreateWaitingAreaResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public int Number { get; init; }

    public string? AudioDevice { get; init; }

    public string? ControlDevice { get; init; }

    public string? DescriptiveName { get; init; }

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.WaitingAreas,
        OperationalCacheTags.Branch(BranchId)
    };
}
