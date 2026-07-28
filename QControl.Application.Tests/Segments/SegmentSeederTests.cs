using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using Qcontrol.infrastructure.Seeders;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Segments;

public sealed class SegmentSeederTests
{
    [Fact]
    public async Task Default_segment_seeder_is_idempotent()
    {
        var segments = new List<Segment>();
        var seeder = new DefaultSegmentSeeder(
            new InMemoryWriteReadRepository<Segment>(segments),
            new InMemoryWriteRepository<Segment>(segments),
            new TestUnitOfWork(),
            NullLogger<DefaultSegmentSeeder>.Instance);

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var segment = Assert.Single(segments);
        Assert.True(segment.IsSystemDefault);
        Assert.Equal(0, segment.Priority);
        Assert.Equal("Default", segment.EnglishName);
    }

    [Fact]
    public async Task Existing_leaf_assignment_receives_default_once()
    {
        var defaultSegment = Segment.CreateSystemDefault(
            "افتراضي",
            "Default",
            EntityTestFactory.CurrentUserId);
        SetId(defaultSegment, 1);
        var service = Service.CreateGlobal(
            parentServiceId: null,
            "خدمة",
            "Service",
            serviceCode: null,
            isServiceCodeRequired: false,
            arabicUserMessage: null,
            englishUserMessage: null,
            isTicketIssuable: true,
            isClientInputRequired: false,
            hasReservation: false,
            orderNo: 0,
            priority: 0,
            rangePrefix: "R",
            rangeStartNumber: 1,
            rangeEndNumber: 100,
            waitingDuration: 0,
            noOfTicketCopies: 1,
            EntityTestFactory.CurrentUserId);
        var branchService = BranchService.Create(
            branchId: 5,
            service,
            EntityTestFactory.CurrentUserId);
        SetId(branchService, 2);
        var assignments = new List<BranchServiceSegment>();
        var seeder = new DefaultBranchServiceSegmentSeeder(
            new InMemoryWriteReadRepository<Segment>(
                new List<Segment> { defaultSegment }),
            new InMemoryWriteReadRepository<BranchService>(
                new List<BranchService> { branchService }),
            new InMemoryWriteReadRepository<BranchServiceSegment>(
                assignments),
            new InMemoryWriteRepository<BranchServiceSegment>(
                assignments),
            new TestUnitOfWork(),
            NullLogger<DefaultBranchServiceSegmentSeeder>.Instance);

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var assignment = Assert.Single(assignments);
        Assert.Equal(2, assignment.BranchServiceId);
        Assert.Equal(1, assignment.SegmentId);
        Assert.Equal(100, assignment.Quota);
    }

    private static void SetId<TEntity>(TEntity entity, int id)
        where TEntity : class
    {
        var type = entity.GetType();
        while (type is not null)
        {
            var field = type.GetField(
                "<Id>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is not null)
            {
                field.SetValue(entity, id);
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException("Id field was not found.");
    }
}
