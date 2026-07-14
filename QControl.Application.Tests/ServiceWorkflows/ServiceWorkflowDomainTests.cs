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
}
