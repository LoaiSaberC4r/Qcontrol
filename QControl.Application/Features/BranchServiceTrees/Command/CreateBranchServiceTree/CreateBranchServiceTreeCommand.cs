using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceTree;

public sealed record CreateBranchServiceTreeCommand
    : ICommand<CreateBranchServiceTreeResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public CreateBranchServiceTreeNodeCommand? Root { get; init; }

    public IEnumerable<string> Tags =>
        new[]
        {
            OperationalCacheTags.Services,
            OperationalCacheTags.ServiceCentral,
            OperationalCacheTags.BranchServices,
            OperationalCacheTags.BranchServicesForBranch(BranchId)
        };
}

public sealed class CreateBranchServiceTreeNodeCommand
{
    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string? ArabicUserMessage { get; init; }

    public string? EnglishUserMessage { get; init; }

    public bool? IsTicketIssuable { get; init; }

    public bool IsClientInputRequired { get; init; }

    public bool HasReservation { get; init; }

    public int OrderNo { get; init; }

    public int Priority { get; init; }

    public string? RangePrefix { get; init; }

    public int? RangeStartNumber { get; init; }

    public int? RangeEndNumber { get; init; }

    public int? WaitingDuration { get; init; }

    public int? NoOfTicketCopies { get; init; }

    public IReadOnlyCollection<CreateBranchServiceTreeNodeCommand> Children { get; init; } =
        Array.Empty<CreateBranchServiceTreeNodeCommand>();
}
