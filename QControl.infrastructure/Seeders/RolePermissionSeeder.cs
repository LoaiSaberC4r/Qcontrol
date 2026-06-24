using Microsoft.Extensions.Logging;
using Qcontrol.Domain.Identity;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Seeding;

namespace Qcontrol.infrastructure.Seeders;

internal sealed class RolePermissionSeeder : ISeeder
{
    public int ExecutionOrder { get; set; } = 4;

    private readonly IWriteReadRepository<Role>
        _roleReadFromWrite;

    private readonly IWriteReadRepository<Permission>
        _permissionReadFromWrite;

    private readonly IWriteReadRepository<RolePermission>
        _rolePermissionReadFromWrite;

    private readonly IWriteRepository<RolePermission>
        _rolePermissionWrite;

    private readonly IUnitOfWork _unitOfWork;

    private readonly ILogger<RolePermissionSeeder> _logger;

    public RolePermissionSeeder(
        IWriteReadRepository<Role> roleReadFromWrite,
        IWriteReadRepository<Permission> permissionReadFromWrite,
        IWriteReadRepository<RolePermission> rolePermissionReadFromWrite,
        IWriteRepository<RolePermission> rolePermissionWrite,
        IUnitOfWork unitOfWork,
        ILogger<RolePermissionSeeder> logger)
    {
        _roleReadFromWrite =
            roleReadFromWrite
            ?? throw new ArgumentNullException(
                nameof(roleReadFromWrite));

        _permissionReadFromWrite =
            permissionReadFromWrite
            ?? throw new ArgumentNullException(
                nameof(permissionReadFromWrite));

        _rolePermissionReadFromWrite =
            rolePermissionReadFromWrite
            ?? throw new ArgumentNullException(
                nameof(rolePermissionReadFromWrite));

        _rolePermissionWrite =
            rolePermissionWrite
            ?? throw new ArgumentNullException(
                nameof(rolePermissionWrite));

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
        foreach (
            var item in
            SeedConstants.SeedCatalog.RolePermissions)
        {
            var roleExists =
                await _roleReadFromWrite.AnyAsync(
                    x => x.Id == item.RoleId);

            if (!roleExists)
            {
                throw new InvalidOperationException(
                    "Cannot seed RolePermission because " +
                    $"RoleId '{item.RoleId}' does not exist.");
            }

            var permissionExists =
                await _permissionReadFromWrite.AnyAsync(
                    x => x.Id == item.PermissionId);

            if (!permissionExists)
            {
                throw new InvalidOperationException(
                    "Cannot seed RolePermission because " +
                    $"PermissionId '{item.PermissionId}' does not exist.");
            }

            var rolePermissionExists =
                await _rolePermissionReadFromWrite.AnyAsync(
                    x =>
                        x.RoleId == item.RoleId &&
                        x.PermissionId == item.PermissionId);

            if (rolePermissionExists)
            {
                _logger.LogInformation(
                    "RolePermission RoleId {RoleId}, PermissionId {PermissionId} already exists. Skipping.",
                    item.RoleId,
                    item.PermissionId);

                continue;
            }

            var rolePermission =
                RolePermission.Create(
                    roleId: item.RoleId,
                    permissionId: item.PermissionId,
                    createdByApplicationUserId:
                        SeedConstants
                            .TechnicalAdminSeed
                            .ApplicationUserId);

            await _rolePermissionWrite.AddAsync(
                rolePermission);

            _logger.LogInformation(
                "RolePermission RoleId {RoleId}, PermissionId {PermissionId} seeded successfully.",
                item.RoleId,
                item.PermissionId);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}