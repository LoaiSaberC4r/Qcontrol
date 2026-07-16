using Qcontrol.Application.Features.ServiceWorkflows.Command.CreateServiceWorkflow;
using Qcontrol.Application.Features.ServiceWorkflows.Command.SetDefaultServiceWorkflow;
using Qcontrol.Application.Features.ServiceWorkflows.Command.UpdateServiceWorkflow;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;

namespace QControl.Application.Tests.ServiceWorkflows;

public sealed class ServiceWorkflowValidatorTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public void Create_allows_repeated_service_ids_with_unique_step_orders()
    {
        var validator = new CreateServiceWorkflowCommandValidator();

        var result = validator.Validate(new CreateServiceWorkflowCommand
        {
            BranchId = 1,
            LeafServiceId = 10,
            ArabicName = "Arabic",
            EnglishName = "English",
            Steps = new[]
            {
                new ServiceWorkflowStepCommandItem
                {
                    ServiceId = 10,
                    StepOrder = 1
                },
                new ServiceWorkflowStepCommandItem
                {
                    ServiceId = 10,
                    StepOrder = 2
                }
            }
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_accepts_omitted_and_null_workflow_names()
    {
        var validator = new CreateServiceWorkflowCommandValidator();

        var omitted = validator.Validate(new CreateServiceWorkflowCommand
        {
            BranchId = 1,
            LeafServiceId = 10,
            Steps = ValidSteps()
        });

        var nullNames = validator.Validate(new CreateServiceWorkflowCommand
        {
            BranchId = 1,
            LeafServiceId = 10,
            ArabicName = null,
            EnglishName = null,
            Steps = ValidSteps()
        });

        Assert.True(omitted.IsValid);
        Assert.True(nullNames.IsValid);
    }

    [Fact]
    public void Create_accepts_whitespace_and_single_language_workflow_names()
    {
        var validator = new CreateServiceWorkflowCommandValidator();

        var whitespace = validator.Validate(new CreateServiceWorkflowCommand
        {
            BranchId = 1,
            LeafServiceId = 10,
            ArabicName = "   ",
            EnglishName = "\t ",
            Steps = ValidSteps()
        });

        var arabicOnly = validator.Validate(new CreateServiceWorkflowCommand
        {
            BranchId = 1,
            LeafServiceId = 10,
            ArabicName = "Arabic",
            EnglishName = null,
            Steps = ValidSteps()
        });

        var englishOnly = validator.Validate(new CreateServiceWorkflowCommand
        {
            BranchId = 1,
            LeafServiceId = 10,
            ArabicName = null,
            EnglishName = "English",
            Steps = ValidSteps()
        });

        Assert.True(whitespace.IsValid);
        Assert.True(arabicOnly.IsValid);
        Assert.True(englishOnly.IsValid);
    }

    [Fact]
    public void Create_rejects_trimmed_workflow_names_over_100_characters()
    {
        var validator = new CreateServiceWorkflowCommandValidator();
        var tooLong = $" {new string('A', 101)} ";

        var arabicResult = validator.Validate(new CreateServiceWorkflowCommand
        {
            BranchId = 1,
            LeafServiceId = 10,
            ArabicName = tooLong,
            EnglishName = null,
            Steps = ValidSteps()
        });

        var englishResult = validator.Validate(new CreateServiceWorkflowCommand
        {
            BranchId = 1,
            LeafServiceId = 10,
            ArabicName = null,
            EnglishName = tooLong,
            Steps = ValidSteps()
        });

        Assert.False(arabicResult.IsValid);
        Assert.Contains(
            arabicResult.Errors,
            error => error.ErrorMessage ==
                ServiceWorkflowMessages.ArabicNameMaxLength);

        Assert.False(englishResult.IsValid);
        Assert.Contains(
            englishResult.Errors,
            error => error.ErrorMessage ==
                ServiceWorkflowMessages.EnglishNameMaxLength);
    }

    [Fact]
    public void Update_accepts_null_workflow_names_with_valid_row_version()
    {
        var validator = new UpdateServiceWorkflowCommandValidator();

        var result = validator.Validate(new UpdateServiceWorkflowCommand
        {
            BranchId = 1,
            LeafServiceId = 10,
            Id = 50,
            ArabicName = null,
            EnglishName = null,
            Steps = ValidSteps(),
            RowVersion = ValidRowVersion
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_rejects_duplicate_step_order()
    {
        var validator = new CreateServiceWorkflowCommandValidator();

        var result = validator.Validate(new CreateServiceWorkflowCommand
        {
            BranchId = 1,
            LeafServiceId = 10,
            ArabicName = "Arabic",
            EnglishName = "English",
            Steps = new[]
            {
                new ServiceWorkflowStepCommandItem
                {
                    ServiceId = 10,
                    StepOrder = 1
                },
                new ServiceWorkflowStepCommandItem
                {
                    ServiceId = 11,
                    StepOrder = 1
                }
            }
        });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage ==
                ServiceWorkflowMessages.StepOrderDuplicate);
    }

    [Fact]
    public void Create_rejects_non_sequential_step_order()
    {
        var validator = new CreateServiceWorkflowCommandValidator();

        var result = validator.Validate(new CreateServiceWorkflowCommand
        {
            BranchId = 1,
            LeafServiceId = 10,
            ArabicName = "Arabic",
            EnglishName = "English",
            Steps = new[]
            {
                new ServiceWorkflowStepCommandItem
                {
                    ServiceId = 10,
                    StepOrder = 1
                },
                new ServiceWorkflowStepCommandItem
                {
                    ServiceId = 11,
                    StepOrder = 3
                }
            }
        });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage ==
                ServiceWorkflowMessages.StepOrderNotSequential);
    }

    [Fact]
    public void Set_default_requires_valid_row_version()
    {
        var validator = new SetDefaultServiceWorkflowCommandValidator();

        var invalid = validator.Validate(new SetDefaultServiceWorkflowCommand
        {
            BranchId = 1,
            LeafServiceId = 10,
            Id = 50,
            RowVersion = "not-base64"
        });

        var valid = validator.Validate(new SetDefaultServiceWorkflowCommand
        {
            BranchId = 1,
            LeafServiceId = 10,
            Id = 50,
            RowVersion = ValidRowVersion
        });

        Assert.False(invalid.IsValid);
        Assert.True(valid.IsValid);
    }

    private static IReadOnlyList<ServiceWorkflowStepCommandItem> ValidSteps()
        => new[]
        {
            new ServiceWorkflowStepCommandItem
            {
                ServiceId = 10,
                StepOrder = 1
            },
            new ServiceWorkflowStepCommandItem
            {
                ServiceId = 11,
                StepOrder = 2
            }
        };
}
