using Qcontrol.Application.Features.Services.Command.CreateService;
using Qcontrol.Application.Features.Services.Command.UpdateService;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.Services;

public sealed class ServiceCustomInputValidationTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public void Create_accepts_valid_string_and_integer_inputs()
    {
        var command = ValidCreate() with
        {
            IsClientInputRequired = true,
            CustomInputs = new ServiceCustomInputDefinitionCommand[]
            {
                StringInput(),
                IntegerInput(order: 2)
            }.Cast<CreateServiceCustomInputCommand>().ToArray()
        };

        var result = new CreateServiceCommandValidator().Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_requires_an_input_when_client_input_is_required()
    {
        var result = new CreateServiceCommandValidator().Validate(
            ValidCreate() with { IsClientInputRequired = true });

        Assert.Contains(
            result.Errors,
            x => x.ErrorMessage.Contains("At least one custom input"));
    }

    [Fact]
    public void Create_rejects_inputs_when_client_input_is_not_required()
    {
        var result = new CreateServiceCommandValidator().Validate(
            ValidCreate() with
            {
                CustomInputs = new[] { StringInput() }
            });

        Assert.Contains(
            result.Errors,
            x => x.ErrorMessage.Contains("are not allowed"));
    }

    [Fact]
    public void Create_rejects_duplicate_trimmed_case_insensitive_names()
    {
        var result = ValidateRequiredInputs(
            StringInput(name: " NationalId ", order: 1),
            StringInput(name: "nationalid", order: 2));

        Assert.Contains(
            result.Errors,
            x => x.ErrorMessage.Contains("names must be unique"));
    }

    [Fact]
    public void Create_rejects_duplicate_orders()
    {
        var result = ValidateRequiredInputs(
            StringInput(name: "First", order: 1),
            IntegerInput(name: "Second", order: 1));

        Assert.Contains(
            result.Errors,
            x => x.ErrorMessage.Contains("orders must be unique"));
    }

    [Fact]
    public void Create_rejects_integer_restrictions_on_string_input()
    {
        var result = ValidateRequiredInputs(
            WithValues(StringInput(), minValue: 1));

        Assert.Contains(
            result.Errors,
            x => x.ErrorMessage.Contains("string custom input"));
    }

    [Fact]
    public void Create_rejects_string_restrictions_on_integer_input()
    {
        var invalid = new CreateServiceCustomInputCommand
        {
            Name = "Age",
            Type = ServiceCustomInputType.Integer,
            MinLength = 1,
            Order = 1
        };

        var result = ValidateRequiredInputs(invalid);

        Assert.Contains(
            result.Errors,
            x => x.ErrorMessage.Contains("integer custom input"));
    }

    [Fact]
    public void Create_rejects_invalid_length_and_value_ranges()
    {
        var invalidLength = ValidateRequiredInputs(
            StringInput(minLength: 10, maxLength: 5));
        var invalidValue = ValidateRequiredInputs(
            IntegerInput(minValue: 100, maxValue: 18));

        Assert.Contains(
            invalidLength.Errors,
            x => x.ErrorMessage.Contains("maximum length"));
        Assert.Contains(
            invalidValue.Errors,
            x => x.ErrorMessage.Contains("maximum value"));
    }

    [Fact]
    public void Create_rejects_whitespace_start_with()
    {
        var result = ValidateRequiredInputs(StringInput(startWith: "   "));

        Assert.Contains(
            result.Errors,
            x => x.ErrorMessage.Contains("cannot be empty"));
    }

    [Fact]
    public void Update_rejects_duplicate_custom_input_ids()
    {
        var command = ValidUpdate() with
        {
            IsClientInputRequired = true,
            CustomInputs = new[]
            {
                UpdateInput(12, "First", 1),
                UpdateInput(12, "Second", 2)
            }
        };

        var result = new UpdateServiceCommandValidator().Validate(command);

        Assert.Contains(
            result.Errors,
            x => x.ErrorMessage.Contains("id cannot appear more than once"));
    }

    private static FluentValidation.Results.ValidationResult
        ValidateRequiredInputs(
            params CreateServiceCustomInputCommand[] inputs) =>
        new CreateServiceCommandValidator().Validate(
            ValidCreate() with
            {
                IsClientInputRequired = true,
                CustomInputs = inputs
            });

    private static CreateServiceCommand ValidCreate() => new()
    {
        ArabicName = "خدمة",
        EnglishName = "Service",
        IsTicketIssuable = false,
        IsClientInputRequired = false,
        OrderNo = 1,
        Priority = 0
    };

    private static UpdateServiceCommand ValidUpdate() => new()
    {
        Id = 1,
        ArabicName = "خدمة",
        EnglishName = "Service",
        IsTicketIssuable = false,
        IsClientInputRequired = false,
        OrderNo = 1,
        Priority = 0,
        RowVersion = ValidRowVersion
    };

    private static CreateServiceCustomInputCommand StringInput(
        string name = "NationalId",
        int order = 1,
        int? minLength = 1,
        int? maxLength = 100,
        string? startWith = null) => new()
        {
            Name = name,
            Type = ServiceCustomInputType.String,
            IsRequired = true,
            MinLength = minLength,
            MaxLength = maxLength,
            StartWith = startWith,
            Order = order
        };

    private static CreateServiceCustomInputCommand IntegerInput(
        string name = "Age",
        int order = 1,
        int? minValue = 18,
        int? maxValue = 100) => new()
        {
            Name = name,
            Type = ServiceCustomInputType.Integer,
            MinValue = minValue,
            MaxValue = maxValue,
            Order = order
        };

    private static CreateServiceCustomInputCommand WithValues(
        CreateServiceCustomInputCommand input,
        int? minValue = null,
        int? maxValue = null) => new()
        {
            Name = input.Name,
            LabelEn = input.LabelEn,
            LabelAr = input.LabelAr,
            Type = input.Type,
            IsRequired = input.IsRequired,
            MinLength = input.MinLength,
            MaxLength = input.MaxLength,
            MinValue = minValue,
            MaxValue = maxValue,
            StartWith = input.StartWith,
            Order = input.Order
        };

    private static UpdateServiceCustomInputCommand UpdateInput(
        int id,
        string name,
        int order) => new()
        {
            CustomInputId = id,
            Name = name,
            Type = ServiceCustomInputType.String,
            Order = order
        };
}
