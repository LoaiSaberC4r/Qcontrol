using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Seeding;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Seeders;

public sealed class DefaultBranchServiceSegmentSeeder : ISeeder
{
    public int ExecutionOrder { get; set; } = 7;

    private readonly IWriteReadRepository<Segment> _segments;
    private readonly IWriteReadRepository<BranchService> _branchServices;
    private readonly IWriteReadRepository<BranchServiceSegment> _assignments;
    private readonly IWriteRepository<BranchServiceSegment> _assignmentWrite;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DefaultBranchServiceSegmentSeeder> _logger;

    public DefaultBranchServiceSegmentSeeder(
        IWriteReadRepository<Segment> segments,
        IWriteReadRepository<BranchService> branchServices,
        IWriteReadRepository<BranchServiceSegment> assignments,
        IWriteRepository<BranchServiceSegment> assignmentWrite,
        IUnitOfWork unitOfWork,
        ILogger<DefaultBranchServiceSegmentSeeder> logger)
    {
        _segments = segments;
        _branchServices = branchServices;
        _assignments = assignments;
        _assignmentWrite = assignmentWrite;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var defaultSegmentId = await _segments.Query()
            .AsNoTracking()
            .Where(x => x.IsSystemDefault)
            .Select(x => x.Id)
            .SingleAsync();
        var existingBranchServiceIds = await _assignments.Query()
            .AsNoTracking()
            .Where(x => x.SegmentId == defaultSegmentId)
            .Select(x => x.BranchServiceId)
            .ToArrayAsync();
        var candidates = await _branchServices.Query()
            .AsNoTracking()
            .Where(x =>
                !existingBranchServiceIds.Contains(x.Id) &&
                x.Service.IsTicketIssuable &&
                !x.Service.Children.Any() &&
                x.Service.RangeStartNumber.HasValue &&
                x.Service.RangeEndNumber.HasValue &&
                x.Service.RangeStartNumber >= 0 &&
                x.Service.RangeEndNumber >= x.Service.RangeStartNumber)
            .Select(x => new
            {
                x.Id,
                Start = x.Service.RangeStartNumber!.Value,
                End = x.Service.RangeEndNumber!.Value
            })
            .ToListAsync();
        var candidateIds = candidates.Select(x => x.Id).ToArray();
        var allocatedTotals = await _assignments.Query()
            .AsNoTracking()
            .Where(x =>
                candidateIds.Contains(x.BranchServiceId) &&
                !x.Segment.IsSystemDefault)
            .GroupBy(x => x.BranchServiceId)
            .Select(x => new
            {
                BranchServiceId = x.Key,
                Total = x.Sum(item => item.Quota)
            })
            .ToDictionaryAsync(x => x.BranchServiceId, x => x.Total);
        if (candidates.Any(x =>
            allocatedTotals.GetValueOrDefault(x.Id) >
                x.End - x.Start + 1))
        {
            throw new InvalidOperationException(
                "A branch service has segment quotas above its capacity.");
        }

        var additions = candidates
            .Select(x => BranchServiceSegment.Create(
                x.Id,
                defaultSegmentId,
                x.End - x.Start + 1 -
                    allocatedTotals.GetValueOrDefault(x.Id),
                SeedConstants.TechnicalAdminSeed.ApplicationUserId))
            .ToList();
        if (additions.Count == 0)
        {
            _logger.LogInformation(
                "All eligible branch services already have a default segment.");
            return;
        }

        await _assignmentWrite.AddRangeAsync(additions);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation(
            "Created {Count} default branch-service segment assignments.",
            additions.Count);
    }
}
