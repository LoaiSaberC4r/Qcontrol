using Microsoft.Extensions.Logging;
using Qcontrol.Domain.Identity;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Seeding;

namespace Qcontrol.infrastructure.Seeders;

internal sealed class RoleSeeder : ISeeder
{
    public int ExecutionOrder { get; set; } = 2;

    private readonly IWriteReadRepository<Role> _readFromWrite;
    private readonly IWriteRepository<Role> _write;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RoleSeeder> _logger;

    public RoleSeeder(
        IWriteReadRepository<Role> readFromWrite,
        IWriteRepository<Role> write,
        IUnitOfWork unitOfWork,
        ILogger<RoleSeeder> logger)
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
        foreach (var item in SeedConstants.SeedCatalog.Roles)
        {
            var existsById =
                await _readFromWrite.AnyAsync(
                    x => x.Id == item.Id);

            if (existsById)
            {
                _logger.LogInformation(
                    "Role {RoleName} already exists by Id. Skipping.",
                    item.Name);

                continue;
            }

            var existsByName =
                await _readFromWrite.AnyAsync(
                    x => x.Name == item.Name);

            if (existsByName)
            {
                throw new InvalidOperationException(
                    $"Role '{item.Name}' already exists with a different Id. " +
                    "Align seed ids before continuing.");
            }

            var role = Role.CreateSeeded(
                id: item.Id,
                name: item.Name,
                createdByApplicationUserId:
                    SeedConstants.TechnicalAdminSeed.ApplicationUserId);

            await _write.AddAsync(role);

            _logger.LogInformation(
                "Role {RoleName} seeded successfully.",
                item.Name);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}