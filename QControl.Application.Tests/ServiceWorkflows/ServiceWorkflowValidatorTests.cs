using Qcontrol.Application.Features.ServiceWorkflows.Command.CreateServiceWorkflow;
using Qcontrol.Application.Features.ServiceWorkflows.Command.SetDefaultServiceWorkflow;
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
}
