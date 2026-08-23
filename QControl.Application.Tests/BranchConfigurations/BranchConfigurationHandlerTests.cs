using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Qcontrol.Application.Features.BranchConfigurations.Command.CreateBranchConfiguration;
using Qcontrol.Application.Features.BranchConfigurations.Command.UpdateBranchConfiguration;
using Qcontrol.Application.Features.BranchConfigurations.Query.GetBranchConfiguration;
using Qcontrol.Application.Features.BranchConfigurations.Shared;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.BranchConfigurations;

public sealed class BranchConfigurationHandlerTests
{
    private static readonly byte[] RowVersion =
        { 1, 2, 3, 4, 5, 6, 7, 8 };

    [Fact]
    public async Task Get_returns_not_found_when_branch_does_not_exist()
    {
        var handler = GetHandler(new(), new());

        var result = await handler.Handle(
            new GetBranchConfigurationQuery { BranchId = 99 },
            CancellationToken.None);

        AssertFailure(result, ErrorType.NotFound, "BranchNotFound");
    }

    [Fact]
    public async Task Get_returns_forbidden_for_cross_branch_access()
    {
        var branches = new List<Branch> { EntityTestFactory.Branch(1) };
        var access = ForbiddenAccess();
        var branchRepository =
            new InMemoryWriteReadRepository<Branch>(branches);
        var handler = new GetBranchConfigurationQueryHandler(
            branchRepository,
            new InMemoryWriteReadRepository<BranchConfiguration>(new()),
            new TestCurrentUser(),
            access);

        var result = await handler.Handle(
            new GetBranchConfigurationQuery { BranchId = 1 },
            CancellationToken.None);

        AssertFailure(result, ErrorType.Security, "ForeignBranch");
        Assert.Equal(0, branchRepository.AnyCallCount);
    }

    [Fact]
    public async Task Get_returns_zero_fallback_when_configuration_is_missing()
    {
        var handler = GetHandler(
            new() { EntityTestFactory.Branch(1) },
            new());

        var result = await handler.Handle(
            new GetBranchConfigurationQuery { BranchId = 1 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.BranchId);
        Assert.Equal(TimeSpan.Zero, result.Value.AllowedTime);
        Assert.False(result.Value.IsConfigured);
        Assert.Null(result.Value.RowVersion);
        Assert.Null(result.Value.Message);
    }

    [Fact]
    public async Task Get_returns_existing_configuration_and_row_version()
    {
        var configuration = EntityTestFactory.BranchConfiguration(
            10,
            1,
            TimeSpan.FromMinutes(30),
            RowVersion);
        var handler = GetHandler(
            new() { EntityTestFactory.Branch(1) },
            new() { configuration });

        var result = await handler.Handle(
            new GetBranchConfigurationQuery { BranchId = 1 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsConfigured);
        Assert.Equal(TimeSpan.FromMinutes(30), result.Value.AllowedTime);
        Assert.Equal(
            Convert.ToBase64String(RowVersion),
            result.Value.RowVersion);
    }

    [Fact]
    public async Task Create_requires_authentication()
    {
        var handler = CreateHandler(
            new() { EntityTestFactory.Branch(1) },
            new(),
            currentUser: new TestCurrentUser
            {
                IsAuthenticated = false,
                UserId = null
            });

        var result = await handler.Handle(
            CreateCommand(),
            CancellationToken.None);

        AssertFailure(result, ErrorType.Unauthorized, "Unauthenticated");
    }

    [Fact]
    public async Task Create_returns_not_found_for_missing_branch()
    {
        var handler = CreateHandler(new(), new());

        var result = await handler.Handle(
            CreateCommand(),
            CancellationToken.None);

        AssertFailure(result, ErrorType.NotFound, "BranchNotFound");
    }

    [Fact]
    public async Task Create_returns_forbidden_for_cross_branch_access()
    {
        var handler = CreateHandler(
            new() { EntityTestFactory.Branch(1) },
            new(),
            accessValidator: ForbiddenAccess());

        var result = await handler.Handle(
            CreateCommand(),
            CancellationToken.None);

        AssertFailure(result, ErrorType.Security, "ForeignBranch");
    }

    [Fact]
    public async Task First_create_persists_configuration()
    {
        var configurations = new List<BranchConfiguration>();
        var writer =
            new InMemoryWriteRepository<BranchConfiguration>(configurations);
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new() { EntityTestFactory.Branch(1) },
            configurations,
            writer,
            unitOfWork: unitOfWork);

        var result = await handler.Handle(
            CreateCommand(TimeSpan.FromMinutes(30)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsConfigured);
        Assert.Equal(TimeSpan.FromMinutes(30), result.Value.AllowedTime);
        Assert.Equal(1, writer.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Single(configurations);
    }

    [Fact]
    public async Task Second_create_returns_conflict_without_upsert()
    {
        var configurations = new List<BranchConfiguration>
        {
            EntityTestFactory.BranchConfiguration(
                10,
                1,
                TimeSpan.FromMinutes(30),
                RowVersion)
        };
        var writer =
            new InMemoryWriteRepository<BranchConfiguration>(configurations);
        var handler = CreateHandler(
            new() { EntityTestFactory.Branch(1) },
            configurations,
            writer);

        var result = await handler.Handle(
            CreateCommand(TimeSpan.FromMinutes(45)),
            CancellationToken.None);

        AssertFailure(result, ErrorType.Conflict, "AlreadyExists");
        Assert.Equal(0, writer.AddCallCount);
        Assert.Equal(TimeSpan.FromMinutes(30), configurations[0].AllowedTime);
    }

    [Fact]
    public async Task Concurrent_duplicate_database_violation_maps_to_conflict()
    {
        var unitOfWork = new TestUnitOfWork
        {
            SaveChangesException = new DbUpdateException(
                "Duplicate branch configuration.",
                CreateUniqueViolation())
        };
        var handler = CreateHandler(
            new() { EntityTestFactory.Branch(1) },
            new(),
            unitOfWork: unitOfWork);

        var result = await handler.Handle(
            CreateCommand(),
            CancellationToken.None);

        AssertFailure(result, ErrorType.Conflict, "AlreadyExists");
    }

    [Fact]
    public async Task Create_handler_maps_missing_time_to_validation_result()
    {
        var handler = CreateHandler(
            new() { EntityTestFactory.Branch(1) },
            new());

        var result = await handler.Handle(
            new CreateBranchConfigurationCommand
            {
                BranchId = 1,
                AllowedTime = null
            },
            CancellationToken.None);

        AssertFailure(result, ErrorType.Validation, "AllowedTimeRequired");
    }

    [Fact]
    public async Task Update_does_not_create_missing_configuration()
    {
        var configurations = new List<BranchConfiguration>();
        var writer =
            new InMemoryWriteRepository<BranchConfiguration>(configurations);
        var handler = UpdateHandler(
            new() { EntityTestFactory.Branch(1) },
            configurations,
            writer);

        var result = await handler.Handle(
            UpdateCommand(),
            CancellationToken.None);

        AssertFailure(result, ErrorType.NotFound, "ConfigurationNotFound");
        Assert.Equal(0, writer.UpdateCallCount);
        Assert.Empty(configurations);
    }

    [Fact]
    public async Task Update_rejects_invalid_row_version_as_validation()
    {
        var configuration = ExistingConfiguration();
        var handler = UpdateHandler(
            new() { EntityTestFactory.Branch(1) },
            new() { configuration });

        var result = await handler.Handle(
            UpdateCommand(rowVersion: "AQ=="),
            CancellationToken.None);

        AssertFailure(result, ErrorType.Validation, "InvalidRowVersion");
        Assert.Equal(TimeSpan.FromMinutes(30), configuration.AllowedTime);
    }

    [Fact]
    public async Task Stale_update_returns_conflict()
    {
        var configuration = ExistingConfiguration();
        var unitOfWork = new TestUnitOfWork
        {
            SaveChangesException = new DbUpdateConcurrencyException()
        };
        var handler = UpdateHandler(
            new() { EntityTestFactory.Branch(1) },
            new() { configuration },
            unitOfWork: unitOfWork);

        var result = await handler.Handle(
            UpdateCommand(TimeSpan.FromMinutes(45)),
            CancellationToken.None);

        AssertFailure(result, ErrorType.Conflict, "ConcurrencyConflict");
    }

    [Fact]
    public async Task Valid_update_sets_original_token_and_returns_configuration()
    {
        var configuration = ExistingConfiguration();
        var configurations = new List<BranchConfiguration> { configuration };
        var writer =
            new InMemoryWriteRepository<BranchConfiguration>(configurations);
        var concurrency = new TestConcurrencyTokenManager();
        var unitOfWork = new TestUnitOfWork();
        var handler = UpdateHandler(
            new() { EntityTestFactory.Branch(1) },
            configurations,
            writer,
            concurrency,
            unitOfWork);

        var result = await handler.Handle(
            UpdateCommand(TimeSpan.FromMinutes(45)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TimeSpan.FromMinutes(45), configuration.AllowedTime);
        Assert.Equal(TimeSpan.FromMinutes(45), result.Value.AllowedTime);
        Assert.Equal(Convert.ToBase64String(RowVersion), result.Value.RowVersion);
        Assert.Equal(RowVersion, concurrency.LastRowVersion);
        Assert.Equal(1, writer.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_returns_forbidden_for_cross_branch_access()
    {
        var handler = UpdateHandler(
            new() { EntityTestFactory.Branch(1) },
            new() { ExistingConfiguration() },
            accessValidator: ForbiddenAccess());

        var result = await handler.Handle(
            UpdateCommand(),
            CancellationToken.None);

        AssertFailure(result, ErrorType.Security, "ForeignBranch");
    }

    private static GetBranchConfigurationQueryHandler GetHandler(
        List<Branch> branches,
        List<BranchConfiguration> configurations) =>
        new(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<BranchConfiguration>(
                configurations),
            new TestCurrentUser(),
            new TestBranchAccessValidator());

    private static CreateBranchConfigurationCommandHandler CreateHandler(
        List<Branch> branches,
        List<BranchConfiguration> configurations,
        InMemoryWriteRepository<BranchConfiguration>? writer = null,
        TestCurrentUser? currentUser = null,
        TestBranchAccessValidator? accessValidator = null,
        TestUnitOfWork? unitOfWork = null) =>
        new(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<BranchConfiguration>(
                configurations),
            writer ??
                new InMemoryWriteRepository<BranchConfiguration>(
                    configurations),
            currentUser ?? new TestCurrentUser(),
            accessValidator ?? new TestBranchAccessValidator(),
            unitOfWork ?? new TestUnitOfWork());

    private static UpdateBranchConfigurationCommandHandler UpdateHandler(
        List<Branch> branches,
        List<BranchConfiguration> configurations,
        InMemoryWriteRepository<BranchConfiguration>? writer = null,
        TestConcurrencyTokenManager? concurrency = null,
        TestUnitOfWork? unitOfWork = null,
        TestCurrentUser? currentUser = null,
        TestBranchAccessValidator? accessValidator = null) =>
        new(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<BranchConfiguration>(
                configurations),
            writer ??
                new InMemoryWriteRepository<BranchConfiguration>(
                    configurations),
            concurrency ?? new TestConcurrencyTokenManager(),
            currentUser ?? new TestCurrentUser(),
            accessValidator ?? new TestBranchAccessValidator(),
            unitOfWork ?? new TestUnitOfWork());

    private static CreateBranchConfigurationCommand CreateCommand(
        TimeSpan? allowedTime = null) =>
        new()
        {
            BranchId = 1,
            AllowedTime = allowedTime ?? TimeSpan.FromMinutes(30)
        };

    private static UpdateBranchConfigurationCommand UpdateCommand(
        TimeSpan? allowedTime = null,
        string? rowVersion = null) =>
        new()
        {
            BranchId = 1,
            AllowedTime = allowedTime ?? TimeSpan.FromMinutes(45),
            RowVersion = rowVersion ?? Convert.ToBase64String(RowVersion)
        };

    private static BranchConfiguration ExistingConfiguration() =>
        EntityTestFactory.BranchConfiguration(
            10,
            1,
            TimeSpan.FromMinutes(30),
            RowVersion);

    private static TestBranchAccessValidator ForbiddenAccess() =>
        new()
        {
            Result = Result.Fail(new Error(
                "Access.ForeignBranch",
                "Forbidden",
                ErrorType.Security))
        };

    private static SqlException CreateUniqueViolation()
    {
        var errorConstructor = typeof(SqlError).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            new[]
            {
                typeof(int),
                typeof(byte),
                typeof(byte),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(int),
                typeof(int),
                typeof(Exception)
            },
            modifiers: null)!;
        var error = (SqlError)errorConstructor.Invoke(new object?[]
        {
            2601,
            (byte)1,
            (byte)14,
            "test-server",
            "Cannot insert duplicate key row with unique index " +
            "'UX_BranchConfiguration_BranchId'.",
            "",
            1,
            0,
            null
        });
        var errors = (SqlErrorCollection)Activator.CreateInstance(
            typeof(SqlErrorCollection),
            nonPublic: true)!;
        typeof(SqlErrorCollection).GetMethod(
                "Add",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(errors, new object[] { error });

        return (SqlException)typeof(SqlException).GetMethod(
                "CreateException",
                BindingFlags.Static | BindingFlags.NonPublic,
                binder: null,
                new[] { typeof(SqlErrorCollection), typeof(string) },
                modifiers: null)!
            .Invoke(null, new object[] { errors, "16.0" })!;
    }

    private static void AssertFailure<T>(
        Result<T> result,
        ErrorType errorType,
        string codeFragment)
    {
        var error = Assert.Single(result.Errors);
        Assert.Equal(errorType, error.Type);
        Assert.Contains(codeFragment, error.Code, StringComparison.Ordinal);
    }
}
