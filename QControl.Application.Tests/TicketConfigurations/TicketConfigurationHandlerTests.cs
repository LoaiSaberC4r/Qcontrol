using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using QControl.Application.Features.TicketConfigurations.Command.CreateTicketConfiguration;
using QControl.Application.Features.TicketConfigurations.Command.UpdateTicketConfiguration;
using QControl.Application.Features.TicketConfigurations.Query.GetTicketConfiguration;
using QControl.Application.Features.TicketConfigurations.Shared;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.TicketConfigurations;

public sealed class TicketConfigurationHandlerTests
{
    private static readonly byte[] RowVersion = [1, 2, 3, 4, 5, 6, 7, 8];

    [Fact]
    public async Task First_create_persists_once_and_second_create_conflicts()
    {
        var branches = new List<Branch> { EntityTestFactory.Branch(1) };
        var configurations = new List<TicketPrintConfiguration>();
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(branches, configurations, unitOfWork: unitOfWork);

        var result = await handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(8, Assert.Single(configurations).Elements.Count);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        var duplicate = await handler.Handle(CreateCommand(), CancellationToken.None);
        AssertFailure(duplicate, ErrorType.Conflict, "AlreadyExists");
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Get_and_update_before_initialization_return_not_found()
    {
        var branches = new List<Branch> { EntityTestFactory.Branch(1) };
        var configurations = new List<TicketPrintConfiguration>();
        var currentUser = new TestCurrentUser();
        var access = new TestBranchAccessValidator();
        var get = new GetTicketConfigurationQueryHandler(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<TicketPrintConfiguration>(configurations),
            currentUser, access);
        var update = UpdateHandler(branches, configurations);

        AssertFailure(await get.Handle(new GetTicketConfigurationQuery(1),
            CancellationToken.None), ErrorType.NotFound, "NotFound");
        AssertFailure(await update.Handle(UpdateCommand(), CancellationToken.None),
            ErrorType.NotFound, "NotFound");
    }

    [Fact]
    public async Task Valid_update_marks_root_modified_and_sets_concurrency_token()
    {
        var branches = new List<Branch> { EntityTestFactory.Branch(1) };
        var configuration = ExistingConfiguration();
        var configurations = new List<TicketPrintConfiguration> { configuration };
        var writer = new InMemoryWriteRepository<TicketPrintConfiguration>(configurations);
        var concurrency = new TestConcurrencyTokenManager();
        var unitOfWork = new TestUnitOfWork();
        var handler = UpdateHandler(branches, configurations, writer, concurrency, unitOfWork);

        var elements = HiddenElements();
        elements[4] = VisibleText(TicketPrintElementType.TicketNumber, 0, 0, 40, 10);
        var result = await handler.Handle(UpdateCommand(elements), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, writer.UpdateCallCount);
        Assert.Equal(1, concurrency.SetOriginalRowVersionCallCount);
        Assert.Equal(RowVersion, concurrency.LastRowVersion);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.True(configuration.Elements.Single(x =>
            x.ElementType == TicketPrintElementType.TicketNumber).IsVisible);
    }

    [Fact]
    public async Task Stale_update_returns_conflict()
    {
        var branches = new List<Branch> { EntityTestFactory.Branch(1) };
        var configurations = new List<TicketPrintConfiguration> { ExistingConfiguration() };
        var handler = UpdateHandler(branches, configurations,
            unitOfWork: new TestUnitOfWork
            {
                SaveChangesException = new DbUpdateConcurrencyException()
            });

        AssertFailure(await handler.Handle(UpdateCommand(), CancellationToken.None),
            ErrorType.Conflict, "ConcurrencyConflict");
    }

    [Fact]
    public async Task Foreign_branch_access_is_rejected_before_persistence()
    {
        var access = new TestBranchAccessValidator
        {
            Result = Result.Fail(new Error("Access.ForeignBranch", "Forbidden",
                ErrorType.Security))
        };
        var configurations = new List<TicketPrintConfiguration>();
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(new List<Branch> { EntityTestFactory.Branch(1) },
            configurations, access, unitOfWork);

        AssertFailure(await handler.Handle(CreateCommand(), CancellationToken.None),
            ErrorType.Security, "ForeignBranch");
        Assert.Empty(configurations);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private static CreateTicketConfigurationCommandHandler CreateHandler(
        List<Branch> branches,
        List<TicketPrintConfiguration> configurations,
        TestBranchAccessValidator? access = null,
        TestUnitOfWork? unitOfWork = null) => new(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<TicketPrintConfiguration>(configurations),
            new InMemoryWriteRepository<TicketPrintConfiguration>(configurations),
            new TestCurrentUser(), access ?? new TestBranchAccessValidator(),
            unitOfWork ?? new TestUnitOfWork());

    private static UpdateTicketConfigurationCommandHandler UpdateHandler(
        List<Branch> branches,
        List<TicketPrintConfiguration> configurations,
        InMemoryWriteRepository<TicketPrintConfiguration>? writer = null,
        TestConcurrencyTokenManager? concurrency = null,
        TestUnitOfWork? unitOfWork = null) => new(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<TicketPrintConfiguration>(configurations),
            writer ?? new InMemoryWriteRepository<TicketPrintConfiguration>(configurations),
            concurrency ?? new TestConcurrencyTokenManager(), new TestCurrentUser(),
            new TestBranchAccessValidator(), unitOfWork ?? new TestUnitOfWork());

    private static CreateTicketConfigurationCommand CreateCommand() => new()
    {
        BranchId = 1,
        TicketWidthMm = 80,
        TicketHeightMm = 120,
        Elements = HiddenElements()
    };

    private static UpdateTicketConfigurationCommand UpdateCommand(
        IReadOnlyCollection<TicketPrintElementInput>? elements = null) => new()
        {
            BranchId = 1,
            TicketWidthMm = 80,
            TicketHeightMm = 120,
            Elements = elements ?? HiddenElements(),
            RowVersion = Convert.ToBase64String(RowVersion)
        };

    private static TicketPrintConfiguration ExistingConfiguration()
    {
        var configuration = TicketPrintConfiguration.Create(1, 80, 120,
            TicketPrintLayoutValidator.ToSettings(HiddenElements()));
        EntityTestFactory.AssignId<TicketPrintConfiguration, int>(configuration, 10);
        return configuration;
    }

    private static TicketPrintElementInput[] HiddenElements() =>
        Enum.GetValues<TicketPrintElementType>()
            .Select(type => new TicketPrintElementInput { ElementType = type })
            .ToArray();

    private static TicketPrintElementInput VisibleText(
        TicketPrintElementType type,
        decimal x,
        decimal y,
        decimal width,
        decimal height) => new()
        {
            ElementType = type,
            IsVisible = true,
            XMm = x,
            YMm = y,
            WidthMm = width,
            HeightMm = height,
            FontSizePt = 12,
            FontWeight = TicketFontWeight.Bold,
            TextAlign = TicketTextAlign.Center,
            Language = TicketPrintLanguage.En
        };

    private static void AssertFailure<T>(Result<T> result, ErrorType type, string code)
    {
        var error = Assert.Single(result.Errors);
        Assert.Equal(type, error.Type);
        Assert.Contains(code, error.Code);
    }
}
