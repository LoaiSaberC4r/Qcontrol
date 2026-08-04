using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.GeneralBrand.Command.CreateGeneralBrand;
using Qcontrol.Application.Features.GeneralBrand.Command.UpdateGeneralBrand;
using QControl.Application.Tests.TestSupport;

namespace QControl.Application.Tests.GeneralBranding;

public sealed class GeneralBrandCommandHandlerTests
{
    [Fact]
    public async Task Create_persists_singleton_once_when_not_configured()
    {
        var items = new List<QControl.Domain.Entities.GeneralBrand>();
        var writeRepository =
            new InMemoryWriteRepository<QControl.Domain.Entities.GeneralBrand>(
                items);
        var unitOfWork = new TestUnitOfWork();
        var handler = new CreateGeneralBrandCommandHandler(
            new InMemoryWriteReadRepository<QControl.Domain.Entities.GeneralBrand>(
                items),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

        var result = await handler.Handle(
            GeneralBrandTestData.CreateCommand(),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var generalBrand = Assert.Single(items);
        Assert.Equal(1, generalBrand.SingletonKey);
        Assert.Equal("#0070C4", generalBrand.MainColor);
        Assert.Equal("اختيار اللغة", generalBrand.LanguageButtonText);
        Assert.Equal(1, writeRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.NotNull(result.Value.RowVersion);
        Assert.Contains(
            "general-brand",
            GeneralBrandTestData.CreateCommand().Tags);
    }

    [Fact]
    public async Task Create_returns_conflict_when_already_configured()
    {
        var items = new List<QControl.Domain.Entities.GeneralBrand>
        {
            EntityTestFactory.GeneralBrand(1, GeneralBrandTestData.Layout())
        };
        var writeRepository =
            new InMemoryWriteRepository<QControl.Domain.Entities.GeneralBrand>(
                items);
        var unitOfWork = new TestUnitOfWork();
        var handler = new CreateGeneralBrandCommandHandler(
            new InMemoryWriteReadRepository<QControl.Domain.Entities.GeneralBrand>(
                items),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

        var result = await handler.Handle(
            GeneralBrandTestData.CreateCommand(),
            CancellationToken.None);

        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Equal("GeneralBrand.AlreadyConfigured", error.Code);
        Assert.Equal(0, writeRepository.AddCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_changes_layout_and_sets_concurrency_token_once()
    {
        var generalBrand = EntityTestFactory.GeneralBrand(
            1,
            GeneralBrandTestData.Layout());
        var items = new List<QControl.Domain.Entities.GeneralBrand>
        {
            generalBrand
        };
        var writeRepository =
            new InMemoryWriteRepository<QControl.Domain.Entities.GeneralBrand>(
                items);
        var tokenManager = new TestConcurrencyTokenManager();
        var unitOfWork = new TestUnitOfWork();
        var handler = new UpdateGeneralBrandCommandHandler(
            new InMemoryWriteReadRepository<QControl.Domain.Entities.GeneralBrand>(
                items),
            writeRepository,
            tokenManager,
            new TestCurrentUser(),
            unitOfWork);

        var result = await handler.Handle(
            GeneralBrandTestData.UpdateCommand() with
            {
                MainColor = "#abcdef",
                FooterButtonText = "  Back  "
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("#ABCDEF", generalBrand.MainColor);
        Assert.Equal("Back", generalBrand.FooterButtonText);
        Assert.Equal(
            EntityTestFactory.CurrentUserId,
            generalBrand.LastModifiedByApplicationUserId);
        Assert.Equal(1, tokenManager.SetOriginalRowVersionCallCount);
        Assert.Equal(1, writeRepository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_returns_not_found_when_missing()
    {
        var items = new List<QControl.Domain.Entities.GeneralBrand>();
        var handler = CreateUpdateHandler(items, new TestUnitOfWork());

        var result = await handler.Handle(
            GeneralBrandTestData.UpdateCommand(),
            CancellationToken.None);

        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Equal("GeneralBrand.NotConfigured", error.Code);
    }

    [Fact]
    public async Task Update_returns_validation_for_invalid_row_version()
    {
        var items = new List<QControl.Domain.Entities.GeneralBrand>
        {
            EntityTestFactory.GeneralBrand(1, GeneralBrandTestData.Layout())
        };
        var handler = CreateUpdateHandler(items, new TestUnitOfWork());

        var result = await handler.Handle(
            GeneralBrandTestData.UpdateCommand() with
            {
                RowVersion = "invalid"
            },
            CancellationToken.None);

        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("GeneralBrand.InvalidRowVersion", error.Code);
    }

    [Fact]
    public async Task Update_returns_conflict_for_stale_row_version()
    {
        var items = new List<QControl.Domain.Entities.GeneralBrand>
        {
            EntityTestFactory.GeneralBrand(1, GeneralBrandTestData.Layout())
        };
        var handler = CreateUpdateHandler(
            items,
            new TestUnitOfWork
            {
                SaveChangesException = new DbUpdateConcurrencyException()
            });

        var result = await handler.Handle(
            GeneralBrandTestData.UpdateCommand(),
            CancellationToken.None);

        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Equal("GeneralBrand.ConcurrencyConflict", error.Code);
    }

    private static UpdateGeneralBrandCommandHandler CreateUpdateHandler(
        List<QControl.Domain.Entities.GeneralBrand> items,
        TestUnitOfWork unitOfWork) =>
        new(
            new InMemoryWriteReadRepository<QControl.Domain.Entities.GeneralBrand>(
                items),
            new InMemoryWriteRepository<QControl.Domain.Entities.GeneralBrand>(
                items),
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            unitOfWork);
}
