using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Services.Command.DeleteService;

public sealed record DeleteServiceCommand
    : ICommand<ServiceDeleteResponse>,
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
