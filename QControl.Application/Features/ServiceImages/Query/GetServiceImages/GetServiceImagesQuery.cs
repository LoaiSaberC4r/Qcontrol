using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.ServiceImages.Query.GetServiceImages;

public sealed record GetServiceImagesQuery
    : IQuery<IReadOnlyList<ServiceImageResponse>>
{
    public int ServiceId { get; init; }

    public ServiceImageType? ImageType { get; init; }
}
