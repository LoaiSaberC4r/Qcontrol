using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Qcontrol.Domain.Identity;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Seeding;
using QControl.Domain.Identity;

namespace Qcontrol.infrastructure.Seeders;

internal sealed class TechnicalAdminSeeder : ISeeder
{
    public int ExecutionOrder { get; set; } = 1;

    private readonly IWriteReadRepository<ApplicationUser>
        _applicationUserReadFromWrite;

    private readonly IWriteRepository<ApplicationUser>
        _applicationUserWrite;

    private readonly IWriteReadRepository<TechnicalAdmin>
        _technicalAdminReadFromWrite;

    private readonly IWriteRepository<TechnicalAdmin>
        _technicalAdminWrite;

    private readonly IUnitOfWork _uow;

    private readonly ILogger<TechnicalAdminSeeder> _logger;

    private readonly PasswordHasher<ApplicationUser> _passwordHasher;

    public TechnicalAdminSeeder(
        IWriteReadRepository<ApplicationUser> applicationUserReadFromWrite,
        IWriteRepository<ApplicationUser> applicationUserWrite,
        IWriteReadRepository<TechnicalAdmin> technicalAdminReadFromWrite,
        IWriteRepository<TechnicalAdmin> technicalAdminWrite,
        IUnitOfWork uow,
        ILogger<TechnicalAdminSeeder> logger)
    {
        _applicationUserReadFromWrite =
            applicationUserReadFromWrite;

        _applicationUserWrite =
            applicationUserWrite;

        _technicalAdminReadFromWrite =
            technicalAdminReadFromWrite;

        _technicalAdminWrite =
            technicalAdminWrite;

        _uow = uow;

        _logger = logger;

        _passwordHasher =
            new PasswordHasher<ApplicationUser>();
    }

    public async Task SeedAsync()
    {
        await SeedTechnicalAdminApplicationUserAsync();

        await SeedTechnicalAdminProfileAsync();

        await _uow.SaveChangesAsync();
    }

    private async Task SeedTechnicalAdminApplicationUserAsync()
    {
        var existsById =
            await _applicationUserReadFromWrite.AnyAsync(
                x =>
                    x.Id ==
                    SeedConstants
                        .TechnicalAdminSeed
                        .ApplicationUserId);

        if (existsById)
        {
            _logger.LogInformation(
                "Technical Admin application user already exists by Id. Skipping.");

            return;
        }

        var existsByUserName =
            await _applicationUserReadFromWrite.AnyAsync(
                x =>
                    x.UserName ==
                    SeedConstants
                        .TechnicalAdminSeed
                        .UserName);

        if (existsByUserName)
        {
            throw new InvalidOperationException(
                $"Application user " +
                $"'{SeedConstants.TechnicalAdminSeed.UserName}' " +
                "already exists with a different Id. " +
                "Align seed ids before continuing.");
        }

        var existsByEmail =
            await _applicationUserReadFromWrite.AnyAsync(
                x =>
                    x.Email ==
                    SeedConstants
                        .TechnicalAdminSeed
                        .Email);

        if (existsByEmail)
        {
            throw new InvalidOperationException(
                $"Application user email " +
                $"'{SeedConstants.TechnicalAdminSeed.Email}' " +
                "already exists with a different Id. " +
                "Align seed ids before continuing.");
        }

        var technicalAdminUser =
            ApplicationUser.CreateSeededTechnicalAdmin(
                id:
                    SeedConstants
                        .TechnicalAdminSeed
                        .ApplicationUserId,

                userName:
                    SeedConstants
                        .TechnicalAdminSeed
                        .UserName,

                email:
                    SeedConstants
                        .TechnicalAdminSeed
                        .Email,

                nameEn:
                    SeedConstants
                        .TechnicalAdminSeed
                        .NameEn);

        var passwordHash =
            _passwordHasher.HashPassword(
                technicalAdminUser,
                SeedConstants
                    .TechnicalAdminSeed
                    .DefaultPassword);

        technicalAdminUser.SetPasswordHash(
            passwordHash);

        await _applicationUserWrite.AddAsync(
            technicalAdminUser);

        _logger.LogInformation(
            "Technical Admin application user seeded successfully.");
    }

    private async Task SeedTechnicalAdminProfileAsync()
    {
        var existsById =
            await _technicalAdminReadFromWrite.AnyAsync(
                x =>
                    x.Id ==
                    SeedConstants
                        .TechnicalAdminSeed
                        .TechnicalAdminId);

        if (existsById)
        {
            _logger.LogInformation(
                "Technical Admin profile already exists by Id. Skipping.");

            return;
        }

        var existsByApplicationUserId =
            await _technicalAdminReadFromWrite.AnyAsync(
                x =>
                    x.ApplicationUserId ==
                    SeedConstants
                        .TechnicalAdminSeed
                        .ApplicationUserId);

        if (existsByApplicationUserId)
        {
            throw new InvalidOperationException(
                "Technical Admin profile already exists " +
                "with a different Id. " +
                "Align seed ids before continuing.");
        }

        var technicalAdmin =
            TechnicalAdmin.CreateSeeded(
                id:
                    SeedConstants
                        .TechnicalAdminSeed
                        .TechnicalAdminId,

                applicationUserId:
                    SeedConstants
                        .TechnicalAdminSeed
                        .ApplicationUserId);

        await _technicalAdminWrite.AddAsync(
            technicalAdmin);

        _logger.LogInformation(
            "Technical Admin profile seeded successfully.");
    }
}