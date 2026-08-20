using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Query.GetBranchDisplayMessages;

internal sealed class GetBranchDisplayMessagesQueryValidator
    : AbstractValidator<GetBranchDisplayMessagesQuery>
{
    public GetBranchDisplayMessagesQueryValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage(ErrorMessage.Branch_Id_Required);
    }
}
