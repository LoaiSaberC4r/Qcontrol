using BuildingBlock.Application.Abstraction;
using Microsoft.AspNetCore.Http;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceImages.Command.UploadServiceLogoImage;

public sealed record UploadServiceLogoImageCommand
    : ICommand<ServiceImageResponse>,
      ICacheInvalidator
{
    public int ServiceId { get; init; }

    public IFormFile? Image { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Services,
        OperationalCacheTags.Service(ServiceId)
    };
}
