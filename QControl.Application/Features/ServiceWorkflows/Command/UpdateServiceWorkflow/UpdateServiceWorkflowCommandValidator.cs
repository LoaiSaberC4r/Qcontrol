using FluentValidation;
using Qcontrol.Application.Features.ServiceWorkflows.Command.CreateServiceWorkflow;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceWorkflows.Command.UpdateServiceWorkflow;

internal sealed class UpdateServiceWorkflowCommandValidator
    : AbstractValidator<UpdateServiceWorkflowCommand>
{
    public UpdateServiceWorkflowCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.IdRequired);

        RuleFor(x => x.RowVersion)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);

        Include(new ServiceWorkflowFieldsValidator<UpdateServiceWorkflowCommand>(
            x => x.ArabicName,
            x => x.EnglishName,
            x => x.Steps));
    }
}
