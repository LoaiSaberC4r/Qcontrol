using FluentValidation;
using Qcontrol.Application.Features.Services.Command.CreateService;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Command.UpdateService;

internal sealed class UpdateServiceCommandValidator
    : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.IdRequired);

        RuleFor(x => x.RowVersion)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(Qcontrol.Domain.Resources.ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(Qcontrol.Domain.Resources.ErrorMessage.RowVersion_Invalid);

        Include(new ServiceUpdateFieldsValidator());
    }

    private sealed class ServiceUpdateFieldsValidator
        : AbstractValidator<UpdateServiceCommand>
    {
        public ServiceUpdateFieldsValidator()
        {
            RuleFor(x => x.ParentServiceId)
                .GreaterThan(0)
                .WithMessage(ServiceFeatureMessages.ParentIdRequired)
                .When(x => x.ParentServiceId.HasValue);

            RuleFor(x => x.ArabicName)
                .Cascade(CascadeMode.Stop)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage(ServiceFeatureMessages.ArabicNameRequired)
                .MaximumLength(100)
                .WithMessage(ServiceFeatureMessages.ArabicNameMaxLength);

            RuleFor(x => x.EnglishName)
                .Cascade(CascadeMode.Stop)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage(ServiceFeatureMessages.EnglishNameRequired)
                .MaximumLength(100)
                .WithMessage(ServiceFeatureMessages.EnglishNameMaxLength);

            RuleFor(x => x.ServiceCode)
                .Must(value =>
                {
                    var normalized = ServiceCodeNormalizer.Normalize(value);
                    return normalized is null ||
                        normalized.Length <= ServiceCodeNormalizer.MaxLength;
                })
                .WithMessage(ServiceFeatureMessages.ServiceCodeMaximumLength);

            RuleFor(x => x.ServiceCode)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .When(x => x.IsServiceCodeRequired)
                .WithMessage(ServiceFeatureMessages.ServiceCodeRequired);

            RuleFor(x => x.ServiceCode)
                .Must(string.IsNullOrWhiteSpace)
                .When(x => !x.IsServiceCodeRequired)
                .WithMessage(ServiceFeatureMessages.ServiceCodeMustBeNull);

            RuleFor(x => x.ArabicUserMessage)
                .MaximumLength(500)
                .WithMessage(ServiceFeatureMessages.ArabicUserMessageMaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.ArabicUserMessage));

            RuleFor(x => x.EnglishUserMessage)
                .MaximumLength(500)
                .WithMessage(ServiceFeatureMessages.EnglishUserMessageMaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.EnglishUserMessage));

            RuleFor(x => x.IsTicketIssuable)
                .NotNull()
                .WithMessage(ServiceFeatureMessages.IsTicketIssuableRequired);

            RuleFor(x => x.OrderNo)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ServiceFeatureMessages.OrderNoNonNegative);

            RuleFor(x => x.Priority)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ServiceFeatureMessages.PriorityNonNegative);

            RuleFor(x => x.RangePrefix)
                .Cascade(CascadeMode.Stop)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage(ServiceFeatureMessages.RangePrefixRequired)
                .When(x => x.IsTicketIssuable == true);

            RuleFor(x => x.RangePrefix)
                .MaximumLength(10)
                .WithMessage(ServiceFeatureMessages.RangePrefixMaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.RangePrefix));

            RuleFor(x => x.RangeStartNumber)
                .NotNull()
                .WithMessage(ServiceFeatureMessages.RangeStartRequired)
                .When(x => x.IsTicketIssuable == true);

            RuleFor(x => x.RangeStartNumber)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ServiceFeatureMessages.RangeStartNonNegative)
                .When(x => x.RangeStartNumber.HasValue);

            RuleFor(x => x.RangeEndNumber)
                .NotNull()
                .WithMessage(ServiceFeatureMessages.RangeEndRequired)
                .When(x => x.IsTicketIssuable == true);

            RuleFor(x => x)
                .Must(x =>
                    !x.RangeStartNumber.HasValue ||
                    !x.RangeEndNumber.HasValue ||
                    x.RangeEndNumber.Value >= x.RangeStartNumber.Value)
                .WithMessage(ServiceFeatureMessages.RangeEndGreaterOrEqualStart);

            RuleFor(x => x.WaitingDuration)
                .NotNull()
                .WithMessage(ServiceFeatureMessages.WaitingDurationRequired)
                .When(x => x.IsTicketIssuable == true);

            RuleFor(x => x.WaitingDuration)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ServiceFeatureMessages.WaitingDurationNonNegative)
                .When(x => x.WaitingDuration.HasValue);

            RuleFor(x => x.NoOfTicketCopies)
                .NotNull()
                .WithMessage(ServiceFeatureMessages.NoOfTicketCopiesRequired)
                .When(x => x.IsTicketIssuable == true);

            RuleFor(x => x.NoOfTicketCopies)
                .GreaterThan(0)
                .WithMessage(ServiceFeatureMessages.NoOfTicketCopiesPositive)
                .When(x => x.NoOfTicketCopies.HasValue);

            RuleFor(x => x)
                .Custom((command, context) =>
                {
                    var error = ServiceCustomInputRuleChecks.Validate(
                        command.CustomInputs?
                            .Cast<ServiceCustomInputDefinitionCommand>()
                            .ToArray(),
                        command.IsClientInputRequired,
                        "Update",
                        validateIds: true);

                    if (error is not null)
                    {
                        context.AddFailure(
                            nameof(command.CustomInputs),
                            error.Message);
                    }
                });
        }
    }
}
