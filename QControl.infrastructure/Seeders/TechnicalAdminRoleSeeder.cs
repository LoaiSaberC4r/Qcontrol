using Microsoft.Extensions.Logging;
using Qcontrol.Domain.Identity;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Seeding;

namespace Qcontrol.infrastructure.Seeders;

internal sealed class TechnicalAdminRoleSeeder : ISeeder
{
    public int ExecutionOrder { get; set; } = 5;

    private readonly IWriteReadRepository<ApplicationUser>
        _applicationUserReadFromWrite;

    private readonly IWriteReadRepository<Role>
        _roleReadFromWrite;

    private readonly IWriteReadRepository<UserRole>
        _userRoleReadFromWrite;

    private readonly IWriteRepository<UserRole>
        _userRoleWrite;

    private readonly IUnitOfWork _unitOfWork;

    private readonly ILogger<TechnicalAdminRoleSeeder>
        _logger;

    public TechnicalAdminRoleSeeder(
        IWriteReadRepository<ApplicationUser>
            applicationUserReadFromWrite,
        IWriteReadRepository<Role>
            roleReadFromWrite,
        IWriteReadRepository<UserRole>
            userRoleReadFromWrite,
        IWriteRepository<UserRole>
            userRoleWrite,
        IUnitOfWork unitOfWork,
        ILogger<TechnicalAdminRoleSeeder> logger)
    {
        _applicationUserReadFromWrite =
            applicationUserReadFromWrite
            ?? throw new ArgumentNullException(
                nameof(applicationUserReadFromWrite));

        _roleReadFromWrite =
            roleReadFromWrite
            ?? throw new ArgumentNullException(
                nameof(roleReadFromWrite));

        _userRoleReadFromWrite =
            userRoleReadFromWrite
            ?? throw new ArgumentNullException(
                nameof(userRoleReadFromWrite));

        _userRoleWrite =
            userRoleWrite
            ?? throw new ArgumentNullException(
                nameof(userRoleWrite));

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
        var technicalAdminUserExists =
            await _applicationUserReadFromWrite.AnyAsync(
                x =>
                    x.Id ==
                    SeedConstants
                        .TechnicalAdminSeed
                        .ApplicationUserId);

        if (!technicalAdminUserExists)
        {
            throw new InvalidOperationException(
                "Cannot assign Technical Administrator role " +
                "because the seeded Technical Admin user does not exist.");
        }

        var technicalAdministratorRoleExists =
            await _roleReadFromWrite.AnyAsync(
                x =>
                    x.Id ==
                    SeedConstants
                        .SeedIds
                        .Roles
                        .TechnicalAdministrator);

        if (!technicalAdministratorRoleExists)
        {
            throw new InvalidOperationException(
                "Cannot assign Technical Administrator role " +
                "because the seeded role does not exist.");
        }

        var userRoleExists =
            await _userRoleReadFromWrite.AnyAsync(
                x =>
                    x.ApplicationUserId ==
                    SeedConstants
                        .TechnicalAdminSeed
                        .ApplicationUserId
                    &&
                    x.RoleId ==
                    SeedConstants
                        .SeedIds
                        .Roles
                        .TechnicalAdministrator);

        if (userRoleExists)
        {
            _logger.LogInformation(
                "Technical Administrator role already assigned to Technical Admin. Skipping.");

            return;
        }

        var userRole = UserRole.Create(
            applicationUserId:
                SeedConstants
                    .TechnicalAdminSeed
                    .ApplicationUserId,

            roleId:
                SeedConstants
                    .SeedIds
                    .Roles
                    .TechnicalAdministrator,

            createdByApplicationUserId:
                SeedConstants
                    .TechnicalAdminSeed
                    .ApplicationUserId);

        await _userRoleWrite.AddAsync(userRole);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Technical Administrator role assigned to Technical Admin successfully.");
    }
}