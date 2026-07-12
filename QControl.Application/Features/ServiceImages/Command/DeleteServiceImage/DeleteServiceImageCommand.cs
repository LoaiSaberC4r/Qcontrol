using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceImages.Command.DeleteServiceImage;

public sealed record DeleteServiceImageCommand
    : ICommand<ServiceImageDeleteResponse>,
      ICacheInvalidator
{
    public int ServiceId { get; init; }

    public int ImageId { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Services,
        OperationalCacheTags.ServiceCentral,
        OperationalCacheTags.BranchServices,
        OperationalCacheTags.Service(ServiceId)
    };
}
