using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchBranding.Command.UpdateBranchTheme;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Branding;

public sealed class UpdateBranchThemeCommandHandlerTests
{
    [Fact]
    public async Task Creates_complete_layout_when_branding_is_missing()
    {
        var brandings = new List<QControl.Domain.Entities.BranchBranding>();
        var writeRepository =
            new InMemoryWriteRepository<QControl.Domain.Entities.BranchBranding>(
                brandings);
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            brandings,
            writeRepository,
            new TestConcurrencyTokenManager(),
            unitOfWork);

        var result = await handler.Handle(
            ValidCommand(),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var branding = Assert.Single(brandings);
        Assert.Equal("#0070C4", branding.MainColor);
        Assert.Equal("اختيار اللغة", branding.LanguageButtonText);
        Assert.Equal(1.8m, branding.ServiceButtonFontSize);
        Assert.True(branding.AllowRequestMoreServices);
        Assert.Equal(1, writeRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Updates_existing_layout_and_preserves_logo()
    {
        var branding = EntityTestFactory.BranchBranding(
            id: 10,
            branchId: 1,
            logoPath: "Branches/1/Logo/logo.png",
            mainColor: "#111111",
            secondaryColor: "#222222",
            backgroundColor: "#333333");
        var brandings = new List<QControl.Domain.Entities.BranchBranding>
        {
            branding
        };
        var writeRepository =
            new InMemoryWriteRepository<QControl.Domain.Entities.BranchBranding>(
                brandings);
        var tokenManager = new TestConcurrencyTokenManager();
        var handler = CreateHandler(
            brandings,
            writeRepository,
            tokenManager,
            new TestUnitOfWork());

        var result = await handler.Handle(
            ValidCommand() with
            {
                RowVersion = Convert.ToBase64String(
                    new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Branches/1/Logo/logo.png", branding.LogoPath);
        Assert.Equal(
            "/Media/Branches/1/Logo/logo.png",
            result.Value.LogoUrl);
        Assert.Equal("#0070C4", branding.MainColor);
        Assert.Equal(1, writeRepository.UpdateCallCount);
        Assert.Equal(1, tokenManager.SetOriginalRowVersionCallCount);
    }

    [Fact]
    public async Task Existing_branding_requires_row_version()
    {
        var brandings = new List<QControl.Domain.Entities.BranchBranding>
        {
            EntityTestFactory.BranchBranding(10, 1)
        };
        var handler = CreateHandler(
            brandings,
            new InMemoryWriteRepository<QControl.Domain.Entities.BranchBranding>(
                brandings),
            new TestConcurrencyTokenManager(),
            new TestUnitOfWork());

        var result = await handler.Handle(
            ValidCommand(),
            CancellationToken.None);

        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("BranchBranding.InvalidRowVersion", error.Code);
    }

    [Fact]
    public async Task Stale_row_version_returns_conflict()
    {
        var brandings = new List<QControl.Domain.Entities.BranchBranding>
        {
            EntityTestFactory.BranchBranding(10, 1)
        };
        var handler = CreateHandler(
            brandings,
            new InMemoryWriteRepository<QControl.Domain.Entities.BranchBranding>(
                brandings),
            new TestConcurrencyTokenManager(),
            new TestUnitOfWork
            {
                SaveChangesException = new DbUpdateConcurrencyException()
            });

        var result = await handler.Handle(
            ValidCommand() with
            {
                RowVersion = Convert.ToBase64String(
                    new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })
            },
            CancellationToken.None);

        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Equal("BranchBranding.ConcurrencyConflict", error.Code);
    }

    private static UpdateBranchThemeCommandHandler CreateHandler(
        List<QControl.Domain.Entities.BranchBranding> brandings,
        InMemoryWriteRepository<QControl.Domain.Entities.BranchBranding>
            writeRepository,
        TestConcurrencyTokenManager tokenManager,
        TestUnitOfWork unitOfWork) =>
        new(
            new InMemoryWriteReadRepository<Branch>(
                new List<Branch> { EntityTestFactory.Branch(1) }),
            new InMemoryWriteReadRepository<QControl.Domain.Entities.BranchBranding>(
                brandings),
            writeRepository,
            tokenManager,
            new TestCurrentUser(),
            unitOfWork);

    private static UpdateBranchThemeCommand ValidCommand() =>
        new()
        {
            BranchId = 1,
            MainColor = "#0070c4",
            SecondaryColor = "#ffffff",
            BackgroundColor = "#ffffff",
            HeaderColor = "#ffffff",
            FooterColor = "#ffffff",
            MainTextColor = "#a7060f",
            ShowLanguagePage = true,
            DefaultLanguageIsArabic = true,
            ShowServiceNavigationPath = true,
            AllowRequestMoreServices = true,
            LanguageButtonBackgroundColor = "#0070c4",
            LanguageButtonTextColor = "#ffffff",
            LanguageButtonWidth = 40m,
            LanguageButtonHeight = 22m,
            LanguageButtonText = "  اختيار اللغة  ",
            ServiceButtonBackgroundColor = "#0070c4",
            ServiceButtonTextColor = "#ffffff",
            ServiceButtonWidth = 40m,
            ServiceButtonHeight = 20m,
            ServiceButtonSpace = 2m,
            ServiceButtonFontSize = 1.8m,
            ServiceButtonText = "اختيار الخدمة",
            KeypadButtonBackgroundColor = "#0070c4",
            KeypadButtonTextColor = "#ffffff",
            KeypadButtonWidth = 45m,
            KeypadButtonHeight = 10m,
            KeypadButtonText = "تأكيد",
            FooterButtonBackgroundColor = "#0070c4",
            FooterButtonTextColor = "#ffffff",
            FooterButtonWidth = 10m,
            FooterButtonHeight = 12m,
            FooterButtonText = "رجوع"
        };
}
