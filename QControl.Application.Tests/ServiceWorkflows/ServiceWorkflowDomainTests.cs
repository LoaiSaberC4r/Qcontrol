using QControl.Domain.Entities;

namespace QControl.Application.Tests.ServiceWorkflows;

public sealed class ServiceWorkflowDomainTests
{
    [Fact]
    public void Create_stores_branch_leaf_owner_and_default_state()
    {
        var workflow = ServiceWorkflow.Create(
            branchId: 10,
            leafServiceId: 100,
            arabicName: " المسار الأساسي ",
            englishName: " Default Workflow ",
            isDefault: true,
            steps: new[]
            {
                new ServiceWorkflowStepData(100, 1),
                new ServiceWorkflowStepData(101, 2)
            },
            createdByApplicationUserId:
            Guid.Parse("11111111-1111-1111-1111-111111111111"));

        Assert.Equal(10, workflow.BranchId);
        Assert.Equal(100, workflow.LeafServiceId);
        Assert.True(workflow.IsDefault);
        Assert.True(workflow.IsActive);
        Assert.Equal("المسار الأساسي", workflow.ArabicName);
        Assert.Equal("Default Workflow", workflow.EnglishName);
        Assert.Equal(new[] { 1, 2 }, workflow.Steps.Select(x => x.StepOrder));
    }

    [Fact]
    public void Mark_and_remove_default_change_only_default_state()
    {
        var workflow = ServiceWorkflow.Create(
            branchId: 10,
            leafServiceId: 100,
            arabicName: "A",
            englishName: "E",
            isDefault: false,
            steps: new[]
            {
                new ServiceWorkflowStepData(100, 1),
                new ServiceWorkflowStepData(101, 2)
            },
            createdByApplicationUserId:
            Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var modifiedBy =
            Guid.Parse("22222222-2222-2222-2222-222222222222");

        workflow.MarkAsDefault(modifiedBy);

        Assert.True(workflow.IsDefault);
        Assert.Equal(modifiedBy, workflow.LastModifiedByApplicationUserId);

        workflow.RemoveDefault(modifiedBy);

        Assert.False(workflow.IsDefault);
        Assert.Equal(10, workflow.BranchId);
        Assert.Equal(100, workflow.LeafServiceId);
    }

    [Fact]
    public void Create_allows_both_workflow_names_to_be_null()
    {
        var workflow = CreateWorkflow(
            arabicName: null,
            englishName: null);

        Assert.Null(workflow.ArabicName);
        Assert.Null(workflow.EnglishName);
    }

    [Fact]
    public void Create_normalizes_empty_and_whitespace_names_to_null()
    {
        var workflow = CreateWorkflow(
            arabicName: "",
            englishName: " \t ");

        Assert.Null(workflow.ArabicName);
        Assert.Null(workflow.EnglishName);
    }

    [Fact]
    public void Create_trims_provided_workflow_names()
    {
        var workflow = CreateWorkflow(
            arabicName: " Arabic ",
            englishName: " English ");

        Assert.Equal("Arabic", workflow.ArabicName);
        Assert.Equal("English", workflow.EnglishName);
    }

    [Fact]
    public void Update_allows_both_workflow_names_to_be_cleared()
    {
        var workflow = CreateWorkflow(
            arabicName: "Arabic",
            englishName: "English");

        workflow.Update(
            arabicName: null,
            englishName: null,
            steps: ValidSteps(),
            lastModifiedByApplicationUserId:
            Guid.Parse("22222222-2222-2222-2222-222222222222"));

        Assert.Null(workflow.ArabicName);
        Assert.Null(workflow.EnglishName);
    }

    [Fact]
    public void Update_normalizes_whitespace_to_null_and_trims_provided_names()
    {
        var workflow = CreateWorkflow(
            arabicName: null,
            englishName: null);

        workflow.Update(
            arabicName: "   ",
            englishName: " English ",
            steps: ValidSteps(),
            lastModifiedByApplicationUserId:
            Guid.Parse("22222222-2222-2222-2222-222222222222"));

        Assert.Null(workflow.ArabicName);
        Assert.Equal("English", workflow.EnglishName);
    }

    private static ServiceWorkflow CreateWorkflow(
        string? arabicName,
        string? englishName)
    {
        return ServiceWorkflow.Create(
            branchId: 10,
            leafServiceId: 100,
            arabicName: arabicName,
            englishName: englishName,
            isDefault: true,
            steps: ValidSteps(),
            createdByApplicationUserId:
            Guid.Parse("11111111-1111-1111-1111-111111111111"));
    }

    private static IReadOnlyList<ServiceWorkflowStepData> ValidSteps()
        => new[]
        {
            new ServiceWorkflowStepData(100, 1),
            new ServiceWorkflowStepData(101, 2)
        };
}
