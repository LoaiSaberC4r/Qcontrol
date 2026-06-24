using Microsoft.Extensions.Logging;
using Qcontrol.Domain.Identity;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Seeding;

namespace Qcontrol.infrastructure.Seeders;

internal sealed class PermissionSeeder : ISeeder
{
    public int ExecutionOrder { get; set; } = 3;

    private readonly IWriteReadRepository<Permission>
        _readFromWrite;

    private readonly IWriteRepository<Permission> _write;

    private readonly IUnitOfWork _unitOfWork;

    private readonly ILogger<PermissionSeeder> _logger;

    public PermissionSeeder(
        IWriteReadRepository<Permission> readFromWrite,
        IWriteRepository<Permission> write,
        IUnitOfWork unitOfWork,
        ILogger<PermissionSeeder> logger)
    {
        _readFromWrite =
            readFromWrite
            ?? throw new ArgumentNullException(
                nameof(readFromWrite));

        _write =
            write
            ?? throw new ArgumentNullException(
                nameof(write));

        _unitOfWork =
            unitOfWork
            ?? throw new ArgumentNullException(
                nameof(unitOfWork));

        _logger =
            logger
            ?? throw new ArgumentNullException(
                nameof(logger));
    }

    public async Task SeedAsync()
    {
        foreach (var item in SeedConstants.SeedCatalog.Permissions)
        {
            var existsById =
                await _readFromWrite.AnyAsync(
                    x => x.Id == item.Id);

            if (existsById)
            {
                _logger.LogInformation(
                    "Permission {PermissionName} already exists by Id. Skipping.",
                    item.Name);

                continue;
            }

            var existsByName =
                await _readFromWrite.AnyAsync(
                    x => x.Name == item.Name);

            if (existsByName)
            {
                throw new InvalidOperationException(
                    $"Permission '{item.Name}' already exists with a different Id. " +
                    "Align seed ids before continuing.");
            }

            var permission = Permission.CreateSeeded(
                id: item.Id,
                name: item.Name,
                createdByApplicationUserId:
                    SeedConstants.TechnicalAdminSeed.ApplicationUserId);

            await _write.AddAsync(permission);

            _logger.LogInformation(
                "Permission {PermissionName} seeded successfully.",
                item.Name);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}