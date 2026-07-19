using Qcontrol.Application.Features.BranchServices.Command.AssignBranchServices;
using Qcontrol.Application.Features.BranchServices.Command.UnassignBranchLeafService;
using QControl.Application.Shared.Security;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.BranchServices;

public sealed class BranchServiceScopeLifecycleTests
{
    [Fact]
    public async Task Assign_global_leaf_creates_missing_global_ancestors()
    {
        var branch = EntityTestFactory.Branch(7);
        var root = EntityTestFactory.Service(1);
        var leaf = EntityTestFactory.Service(2, root.Id);
        var assignments = new List<BranchService>();
        var writeRepository =
            new InMemoryWriteRepository<BranchService>(assignments);
        var unitOfWork = new TestUnitOfWork();
        var handler = AssignHandler(
            new List<Branch> { branch },
            new List<Service> { root, leaf },
            assignments,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new AssignBranchServicesCommand
            {
                BranchId = branch.Id,
                ServiceIds = new[] { leaf.Id }
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new[] { root.Id, leaf.Id },
            assignments.Select(item => item.ServiceId).OrderBy(id => id));
        Assert.Equal(1, writeRepository.AddRangeCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_is_idempotent_for_existing_global_path()
    {
        var branch = EntityTestFactory.Branch(7);
        var root = EntityTestFactory.Service(1);
        var leaf = EntityTestFactory.Service(2, root.Id);
        var assignments = new List<BranchService>
        {
            EntityTestFactory.BranchService(1, branch.Id, root.Id),
            EntityTestFactory.BranchService(2, branch.Id, leaf.Id)
        };
        var writeRepository =
            new InMemoryWriteRepository<BranchService>(assignments);
        var unitOfWork = new TestUnitOfWork();
        var handler = AssignHandler(
            new List<Branch> { branch },
            new List<Service> { root, leaf },
            assignments,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new AssignBranchServicesCommand
            {
                BranchId = branch.Id,
                ServiceIds = new[] { leaf.Id }
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, assignments.Count);
        Assert.Equal(0, writeRepository.AddRangeCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_mixed_scopes_is_rejected_atomically()
    {
        var branch = EntityTestFactory.Branch(7);
        var globalLeaf = EntityTestFactory.Service(1, isTicketIssuable: true);
        var branchLeaf = EntityTestFactory.BranchScopedService(
            2,
            branch.Id,
            isTicketIssuable: true);
        var assignments = new List<BranchService>();
        var writeRepository =
            new InMemoryWriteRepository<BranchService>(assignments);
        var unitOfWork = new TestUnitOfWork();
        var handler = AssignHandler(
            new List<Branch> { branch },
            new List<Service> { globalLeaf, branchLeaf },
            assignments,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new AssignBranchServicesCommand
            {
                BranchId = branch.Id,
                ServiceIds = new[] { globalLeaf.Id, branchLeaf.Id }
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "BranchServices.Assign.BranchScopedServiceNotSupported");
        Assert.Empty(assignments);
        Assert.Equal(0, writeRepository.AddRangeCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_global_leaf_with_branch_scoped_ancestor_is_rejected()
    {
        var branch = EntityTestFactory.Branch(7);
        var branchParent = EntityTestFactory.BranchScopedService(1, branch.Id);
        var globalLeaf = EntityTestFactory.Service(
            2,
            branchParent.Id,
            isTicketIssuable: true);
        var assignments = new List<BranchService>();
        var unitOfWork = new TestUnitOfWork();
        var handler = AssignHandler(
            new List<Branch> { branch },
            new List<Service> { branchParent, globalLeaf },
            assignments,
            new InMemoryWriteRepository<BranchService>(assignments),
            unitOfWork);

        var result = await handler.Handle(
            new AssignBranchServicesCommand
            {
                BranchId = branch.Id,
                ServiceIds = new[] { globalLeaf.Id }
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "BranchServices.Assign.InvalidScopeHierarchy");
        Assert.Empty(assignments);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Unassign_global_leaf_removes_only_unused_global_path()
    {
        var branch = EntityTestFactory.Branch(7);
        var root = EntityTestFactory.Service(1);
        var leafA = EntityTestFactory.Service(2, root.Id);
        var leafB = EntityTestFactory.Service(3, root.Id);
        var assignments = new List<BranchService>
        {
            EntityTestFactory.BranchService(1, branch.Id, root.Id),
            EntityTestFactory.BranchService(2, branch.Id, leafA.Id),
            EntityTestFactory.BranchService(3, branch.Id, leafB.Id)
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = UnassignHandler(
            new List<Branch> { branch },
            new List<Service> { root, leafA, leafB },
            assignments,
            new InMemoryWriteRepository<BranchService>(assignments),
            unitOfWork);

        var result = await handler.Handle(
            new UnassignBranchLeafServiceCommand
            {
                BranchId = branch.Id,
                LeafServiceId = leafA.Id
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(assignments, item => item.ServiceId == leafA.Id);
        Assert.Contains(assignments, item => item.ServiceId == leafB.Id);
        Assert.Contains(assignments, item => item.ServiceId == root.Id);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.False(root.IsDeleted);
        Assert.False(leafA.IsDeleted);
    }

    [Fact]
    public async Task Unassign_global_leaf_removes_unused_ancestors_and_preserves_other_branch()
    {
        var branch = EntityTestFactory.Branch(7);
        var otherBranch = EntityTestFactory.Branch(8);
        var root = EntityTestFactory.Service(1);
        var leaf = EntityTestFactory.Service(2, root.Id);
        var assignments = new List<BranchService>
        {
            EntityTestFactory.BranchService(1, branch.Id, root.Id),
            EntityTestFactory.BranchService(2, branch.Id, leaf.Id),
            EntityTestFactory.BranchService(3, otherBranch.Id, root.Id),
            EntityTestFactory.BranchService(4, otherBranch.Id, leaf.Id)
        };
        var handler = UnassignHandler(
            new List<Branch> { branch, otherBranch },
            new List<Service> { root, leaf },
            assignments,
            new InMemoryWriteRepository<BranchService>(assignments),
            new TestUnitOfWork());

        var result = await handler.Handle(
            new UnassignBranchLeafServiceCommand
            {
                BranchId = branch.Id,
                LeafServiceId = leaf.Id
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(assignments, item => item.BranchId == branch.Id);
        Assert.Equal(
            2,
            assignments.Count(item => item.BranchId == otherBranch.Id));
    }

    [Fact]
    public async Task Unassign_branch_scoped_leaf_is_rejected_without_mutation()
    {
        var branch = EntityTestFactory.Branch(7);
        var leaf = EntityTestFactory.BranchScopedService(
            1,
            branch.Id,
            isTicketIssuable: true);
        var assignments = new List<BranchService>
        {
            EntityTestFactory.BranchService(1, branch.Id, leaf.Id)
        };
        var writeRepository =
            new InMemoryWriteRepository<BranchService>(assignments);
        var unitOfWork = new TestUnitOfWork();
        var handler = UnassignHandler(
            new List<Branch> { branch },
            new List<Service> { leaf },
            assignments,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new UnassignBranchLeafServiceCommand
            {
                BranchId = branch.Id,
                LeafServiceId = leaf.Id
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "BranchServices.Unassign.BranchScopedServiceNotSupported");
        Assert.Single(assignments);
        Assert.Equal(0, writeRepository.DeleteCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Unassign_invalid_cross_scope_path_is_rejected_atomically()
    {
        var branch = EntityTestFactory.Branch(7);
        var parent = EntityTestFactory.BranchScopedService(1, branch.Id);
        var leaf = EntityTestFactory.Service(2, parent.Id);
        var assignments = new List<BranchService>
        {
            EntityTestFactory.BranchService(1, branch.Id, parent.Id),
            EntityTestFactory.BranchService(2, branch.Id, leaf.Id)
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = UnassignHandler(
            new List<Branch> { branch },
            new List<Service> { parent, leaf },
            assignments,
            new InMemoryWriteRepository<BranchService>(assignments),
            unitOfWork);

        var result = await handler.Handle(
            new UnassignBranchLeafServiceCommand
            {
                BranchId = branch.Id,
                LeafServiceId = leaf.Id
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "BranchServices.Unassign.InvalidScopeHierarchy");
        Assert.Equal(2, assignments.Count);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private static AssignBranchServicesCommandHandler AssignHandler(
        List<Branch> branches,
        List<Service> services,
        List<BranchService> assignments,
        InMemoryWriteRepository<BranchService> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<Service>(services),
            new InMemoryWriteReadRepository<BranchService>(assignments),
            writeRepository,
            new TestCurrentUser(),
            AllowAllServiceDefinitionAccessValidator.Instance,
            unitOfWork);

    private static UnassignBranchLeafServiceCommandHandler UnassignHandler(
        List<Branch> branches,
        List<Service> services,
        List<BranchService> assignments,
        InMemoryWriteRepository<BranchService> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<Service>(services),
            new InMemoryWriteReadRepository<BranchService>(assignments),
            writeRepository,
            new TestCurrentUser(),
            AllowAllServiceDefinitionAccessValidator.Instance,
            unitOfWork);
}
