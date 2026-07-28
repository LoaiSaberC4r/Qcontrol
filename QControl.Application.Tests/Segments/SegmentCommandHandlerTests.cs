using System.Reflection;
using Qcontrol.Application.Features.Segments.Command.CreateBranchSegment;
using Qcontrol.Application.Features.Segments.Command.CreateGlobalSegment;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.Segments;

public sealed class SegmentCommandHandlerTests
{
    [Fact]
    public async Task Technical_admin_creates_global_segment()
    {
        var segments = new List<Segment>
        {
            Segment.CreateSystemDefault(
                "افتراضي",
                "Default",
                EntityTestFactory.CurrentUserId)
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = new CreateGlobalSegmentCommandHandler(
            new InMemoryWriteReadRepository<Segment>(segments),
            new InMemoryWriteRepository<Segment>(segments),
            new TestCurrentUser(),
            new TestCurrentBranchContext(),
            unitOfWork);

        var result = await handler.Handle(
            new CreateGlobalSegmentCommand
            {
                ArabicName = "فحص",
                EnglishName = "Examination",
                Priority = 1
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SegmentScope.Global, result.Value.Scope);
        Assert.Equal(2, segments.Count);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Branch_admin_cannot_create_global_segment()
    {
        var segments = new List<Segment>();
        var handler = new CreateGlobalSegmentCommandHandler(
            new InMemoryWriteReadRepository<Segment>(segments),
            new InMemoryWriteRepository<Segment>(segments),
            new TestCurrentUser(),
            new TestCurrentBranchContext
            {
                UserType = UserType.BranchAdmin,
                ActiveBranchId = 5
            },
            new TestUnitOfWork());

        var result = await handler.Handle(
            new CreateGlobalSegmentCommand
            {
                ArabicName = "فحص",
                EnglishName = "Examination",
                Priority = 0
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "Segments.TechnicalAdminRequired",
            result.Errors.Single().Code);
    }

    [Fact]
    public async Task Priority_above_persisted_count_is_rejected()
    {
        var segments = new List<Segment>
        {
            Segment.CreateSystemDefault(
                "افتراضي",
                "Default",
                EntityTestFactory.CurrentUserId)
        };
        var handler = new CreateGlobalSegmentCommandHandler(
            new InMemoryWriteReadRepository<Segment>(segments),
            new InMemoryWriteRepository<Segment>(segments),
            new TestCurrentUser(),
            new TestCurrentBranchContext(),
            new TestUnitOfWork());

        var result = await handler.Handle(
            new CreateGlobalSegmentCommand
            {
                ArabicName = "فحص",
                EnglishName = "Examination",
                Priority = 2
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "Segments.PriorityExceedsSegmentCount",
            result.Errors.Single().Code);
    }

    [Fact]
    public async Task Duplicate_priority_is_accepted()
    {
        var segments = new List<Segment>
        {
            Segment.CreateSystemDefault(
                "افتراضي",
                "Default",
                EntityTestFactory.CurrentUserId),
            Segment.CreateGlobal(
                "أ",
                "A",
                1,
                EntityTestFactory.CurrentUserId)
        };
        var handler = new CreateGlobalSegmentCommandHandler(
            new InMemoryWriteReadRepository<Segment>(segments),
            new InMemoryWriteRepository<Segment>(segments),
            new TestCurrentUser(),
            new TestCurrentBranchContext(),
            new TestUnitOfWork());

        var result = await handler.Handle(
            new CreateGlobalSegmentCommand
            {
                ArabicName = "ب",
                EnglishName = "B",
                Priority = 1
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, segments.Count(x => x.Priority == 1));
    }

    [Fact]
    public async Task Branch_creation_also_creates_pending_request()
    {
        var branches = new List<Branch>
        {
            EntityTestFactory.Branch(5)
        };
        var segments = new List<Segment>
        {
            Segment.CreateSystemDefault(
                "افتراضي",
                "Default",
                EntityTestFactory.CurrentUserId)
        };
        var requests = new List<SegmentGlobalizationRequest>();
        var unitOfWork = new TestUnitOfWork();
        var handler = new CreateBranchSegmentCommandHandler(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<Segment>(segments),
            new InMemoryWriteRepository<Segment>(segments),
            new InMemoryWriteRepository<SegmentGlobalizationRequest>(
                requests),
            new TestCurrentUser(),
            new TestCurrentBranchContext
            {
                UserType = UserType.BranchAdmin,
                ActiveBranchId = 5
            },
            new TestDateTimeProvider(),
            unitOfWork);

        var result = await handler.Handle(
            new CreateBranchSegmentCommand
            {
                BranchId = 5,
                ArabicName = "خاص",
                EnglishName = "Branch",
                Priority = 1
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(requests);
        Assert.Equal(
            SegmentGlobalizationRequestStatus.Pending,
            requests[0].Status);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }
}
