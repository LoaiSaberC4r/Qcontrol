using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Branches.Command.CreateBranch;

public sealed record CreateBranchCommand
    : ICommand<CreateBranchResponse>
{
    public string? ArabicName { get; init; }

    public string? EnglishName { get; init; }

    public string IPAddress { get; init; } = string.Empty;

    public string? License { get; init; }

    public string? Governorate { get; init; }

    public string? City { get; init; }

    public string? Area { get; init; }

    public string? Address { get; init; }

    public string? Longitude { get; init; }

    public string? Latitude { get; init; }
}