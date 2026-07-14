using FluentValidation;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceWorkflows.Command.SetDefaultServiceWorkflow;

internal sealed class SetDefaultServiceWorkflowCommandValidator
    : AbstractValidator<SetDefaultServiceWorkflowCommand>
{
    public SetDefaultServiceWorkflowCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.BranchIdRequired);

        RuleFor(x => x.LeafServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.LeafServiceIdRequired);

        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.IdRequired);

        RuleFor(x => x.RowVersion)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
