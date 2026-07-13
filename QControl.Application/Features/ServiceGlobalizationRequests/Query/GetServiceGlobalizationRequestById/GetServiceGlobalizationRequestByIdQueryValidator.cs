using FluentValidation;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Query.GetServiceGlobalizationRequestById;

internal sealed class GetServiceGlobalizationRequestByIdQueryValidator
    : AbstractValidator<GetServiceGlobalizationRequestByIdQuery>
{
    public GetServiceGlobalizationRequestByIdQueryValidator()
    {
        RuleFor(x => x.RequestId)
            .GreaterThan(0)
            .WithMessage(ServiceGlobalizationRequestMessages.RequestIdRequired);
    }
}
