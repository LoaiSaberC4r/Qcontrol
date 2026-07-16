using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.ServiceWorkflows;

public sealed class ServiceWorkflowRuleCheckTests
{
    [Fact]
    public async Task Validate_duplicate_names_skips_queries_when_both_names_are_null()
    {
        var repository = new InMemoryWriteReadRepository<ServiceWorkflow>(
            new List<ServiceWorkflow>());

        var error = await ServiceWorkflowRuleChecks.ValidateDuplicateNamesAsync(
            repository,
            branchId: 1,
            leafServiceId: 10,
            arabicName: null,
            englishName: null,
            excludedWorkflowId: null,
            operation: "Create",
            cancellationToken: CancellationToken.None);

        Assert.Null(error);
        Assert.Equal(0, repository.FirstOrDefaultProjectionSpecCallCount);
    }

    [Fact]
    public async Task Validate_duplicate_names_checks_only_present_names()
    {
        var arabicOnlyRepository =
            new InMemoryWriteReadRepository<ServiceWorkflow>(
                new List<ServiceWorkflow>());
        var englishOnlyRepository =
            new InMemoryWriteReadRepository<ServiceWorkflow>(
                new List<ServiceWorkflow>());
        var bothRepository = new InMemoryWriteReadRepository<ServiceWorkflow>(
            new List<ServiceWorkflow>());

        await ServiceWorkflowRuleChecks.ValidateDuplicateNamesAsync(
            arabicOnlyRepository,
            branchId: 1,
            leafServiceId: 10,
            arabicName: "Arabic",
            englishName: null,
            excludedWorkflowId: null,
            operation: "Create",
            cancellationToken: CancellationToken.None);

        await ServiceWorkflowRuleChecks.ValidateDuplicateNamesAsync(
            englishOnlyRepository,
            branchId: 1,
            leafServiceId: 10,
            arabicName: null,
            englishName: "English",
            excludedWorkflowId: null,
            operation: "Create",
            cancellationToken: CancellationToken.None);

        await ServiceWorkflowRuleChecks.ValidateDuplicateNamesAsync(
            bothRepository,
            branchId: 1,
            leafServiceId: 10,
            arabicName: "Arabic",
            englishName: "English",
            excludedWorkflowId: null,
            operation: "Create",
            cancellationToken: CancellationToken.None);

        Assert.Equal(1, arabicOnlyRepository.FirstOrDefaultProjectionSpecCallCount);
        Assert.Equal(1, englishOnlyRepository.FirstOrDefaultProjectionSpecCallCount);
        Assert.Equal(2, bothRepository.FirstOrDefaultProjectionSpecCallCount);
    }
}
