using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Branches.Command.CreateBranch;

public sealed record CreateBranchCommand
    : ICommand<CreateBranchResponse>,
      ICacheInvalidator
{
    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string? License { get; init; }

    public string Governorate { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string Area { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public decimal Latitude { get; init; }

    public decimal Longitude { get; init; }

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Branches
    };
}
