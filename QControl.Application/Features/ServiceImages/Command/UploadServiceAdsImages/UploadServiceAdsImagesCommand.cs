using BuildingBlock.Application.Abstraction;
using Microsoft.AspNetCore.Http;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceImages.Command.UploadServiceAdsImages;

public sealed record UploadServiceAdsImagesCommand
    : ICommand<IReadOnlyList<ServiceImageResponse>>,
      ICacheInvalidator
{
    public int ServiceId { get; init; }

    public List<IFormFile> Images { get; init; } = new();

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Services,
        OperationalCacheTags.ServiceCentral,
        OperationalCacheTags.BranchServices,
        OperationalCacheTags.Service(ServiceId)
    };
}
