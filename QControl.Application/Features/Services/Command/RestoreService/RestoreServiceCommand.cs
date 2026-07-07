using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Services.Command.RestoreService;

public sealed record RestoreServiceCommand
    : ICommand<ServiceRestoreResponse>,
      ICacheInvalidator
{
    public int Id { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Services,
        OperationalCacheTags.Service(Id)
    };
}
