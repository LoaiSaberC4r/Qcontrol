using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Seeding;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Seeders;

public sealed class DefaultSegmentSeeder : ISeeder
{
    public int ExecutionOrder { get; set; } = 6;

    private readonly IWriteReadRepository<Segment> _segments;
    private readonly IWriteRepository<Segment> _segmentWrite;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DefaultSegmentSeeder> _logger;

    public DefaultSegmentSeeder(
        IWriteReadRepository<Segment> segments,
        IWriteRepository<Segment> segmentWrite,
        IUnitOfWork unitOfWork,
        ILogger<DefaultSegmentSeeder> logger)
    {
        _segments = segments;
        _segmentWrite = segmentWrite;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var existing = await _segments.Query()
            .AsTracking()
            .SingleOrDefaultAsync(x => x.IsSystemDefault);
        if (existing is not null)
        {
            existing.EnsureSystemValues();
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation(
                "The system-default segment already exists.");
            return;
        }

        var segment = Segment.CreateSystemDefault(
            "افتراضي",
            "Default",
            SeedConstants.TechnicalAdminSeed.ApplicationUserId);
        await _segmentWrite.AddAsync(segment);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("The system-default segment was created.");
    }
}
