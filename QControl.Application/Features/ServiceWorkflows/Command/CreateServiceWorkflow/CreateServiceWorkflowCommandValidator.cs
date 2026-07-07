using FluentValidation;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using System.Linq.Expressions;

namespace Qcontrol.Application.Features.ServiceWorkflows.Command.CreateServiceWorkflow;

internal sealed class CreateServiceWorkflowCommandValidator
    : AbstractValidator<CreateServiceWorkflowCommand>
{
    public CreateServiceWorkflowCommandValidator()
    {
        Include(new ServiceWorkflowFieldsValidator<CreateServiceWorkflowCommand>(
            x => x.ArabicName,
            x => x.EnglishName,
            x => x.Steps));
    }
}

internal sealed class ServiceWorkflowFieldsValidator<T>
    : AbstractValidator<T>
{
    public ServiceWorkflowFieldsValidator(
        Expression<Func<T, string>> arabicName,
        Expression<Func<T, string>> englishName,
        Expression<Func<T, IReadOnlyList<ServiceWorkflowStepCommandItem>?>>
            steps)
    {
        RuleFor(arabicName)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ServiceWorkflowMessages.ArabicNameRequired)
            .MaximumLength(100)
            .WithMessage(ServiceWorkflowMessages.ArabicNameMaxLength);

        RuleFor(englishName)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ServiceWorkflowMessages.EnglishNameRequired)
            .MaximumLength(100)
            .WithMessage(ServiceWorkflowMessages.EnglishNameMaxLength);

        RuleFor(steps)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(ServiceWorkflowMessages.StepsRequired)
            .Must(x => x!.Count > 0)
            .WithMessage(ServiceWorkflowMessages.StepsRequired)
            .Must(x => x!.Count >= 2)
            .WithMessage(ServiceWorkflowMessages.MinimumStepsRequired)
            .Must(HaveUniqueStepOrders)
            .WithMessage(ServiceWorkflowMessages.StepOrderDuplicate)
            .Must(HaveSequentialStepOrders)
            .WithMessage(ServiceWorkflowMessages.StepOrderNotSequential);

        RuleFor(steps)
            .Must(HaveValidServiceIds)
            .WithMessage(ServiceWorkflowMessages.ServiceIdRequired)
            .When(x => steps.Compile()(x) is not null);

        RuleFor(steps)
            .Must(HaveValidStepOrders)
            .WithMessage(ServiceWorkflowMessages.StepOrderInvalid)
            .When(x => steps.Compile()(x) is not null);
    }

    private static bool HaveUniqueStepOrders(
        IReadOnlyList<ServiceWorkflowStepCommandItem>? steps)
    {
        return steps is not null &&
            steps.Select(x => x.StepOrder).Distinct().Count() == steps.Count;
    }

    private static bool HaveSequentialStepOrders(
        IReadOnlyList<ServiceWorkflowStepCommandItem>? steps)
    {
        if (steps is null || steps.Count == 0)
        {
            return false;
        }

        var ordered = steps
            .Select(x => x.StepOrder)
            .OrderBy(x => x)
            .ToList();

        return ordered.SequenceEqual(Enumerable.Range(1, steps.Count));
    }

    private static bool HaveValidServiceIds(
        IReadOnlyList<ServiceWorkflowStepCommandItem>? steps)
    {
        return steps is not null && steps.All(x => x.ServiceId > 0);
    }

    private static bool HaveValidStepOrders(
        IReadOnlyList<ServiceWorkflowStepCommandItem>? steps)
    {
        return steps is not null && steps.All(x => x.StepOrder > 0);
    }
}
