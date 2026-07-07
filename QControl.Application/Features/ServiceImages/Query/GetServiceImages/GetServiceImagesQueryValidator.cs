using FluentValidation;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.ServiceImages.Query.GetServiceImages;

internal sealed class GetServiceImagesQueryValidator
    : AbstractValidator<GetServiceImagesQuery>
{
    public GetServiceImagesQueryValidator()
    {
        RuleFor(x => x.ServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.IdRequired);

        RuleFor(x => x.ImageType)
            .Must(x => !x.HasValue || Enum.IsDefined(typeof(ServiceImageType), x.Value))
            .WithMessage(ServiceFeatureMessages.InvalidImageTypeFilter);
    }
}
