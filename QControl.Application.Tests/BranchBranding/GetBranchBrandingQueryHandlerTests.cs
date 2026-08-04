using Qcontrol.Application.Features.BranchBranding.Query.GetBranchBranding;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Branding;

public sealed class GetBranchBrandingQueryHandlerTests
{
    [Fact]
    public async Task Returns_complete_branch_configuration()
    {
        var branding = EntityTestFactory.BranchBranding(
            10,
            1,
            logoPath: "Branches/1/Logo/logo.png");
        branding.UpdateLayout(Layout(), EntityTestFactory.CurrentUserId);
        var handler = CreateHandler(new() { branding });

        var result = await handler.Handle(
            new GetBranchBrandingQuery { BranchId = 1 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsConfigured);
        Assert.Equal(1, result.Value.BranchId);
        Assert.Equal("/Media/Branches/1/Logo/logo.png", result.Value.LogoUrl);
        Assert.Equal("#112233", result.Value.HeaderColor);
        Assert.True(result.Value.ShowLanguagePage);
        Assert.Equal(40m, result.Value.LanguageButtonWidth);
        Assert.Equal("اختيار اللغة", result.Value.LanguageButtonText);
        Assert.Equal(1.8m, result.Value.ServiceButtonFontSize);
        Assert.Equal("رجوع", result.Value.FooterButtonText);
        Assert.NotNull(result.Value.RowVersion);
    }

    [Fact]
    public async Task Missing_branch_branding_remains_not_configured()
    {
        var handler = CreateHandler(new());

        var result = await handler.Handle(
            new GetBranchBrandingQuery { BranchId = 1 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsConfigured);
        Assert.Equal(1, result.Value.BranchId);
        Assert.Null(result.Value.LogoUrl);
        Assert.Null(result.Value.RowVersion);
    }

    private static GetBranchBrandingQueryHandler CreateHandler(
        List<QControl.Domain.Entities.BranchBranding> brandings) =>
        new(
            new InMemoryWriteReadRepository<Branch>(
                new List<Branch> { EntityTestFactory.Branch(1) }),
            new InMemoryWriteReadRepository<QControl.Domain.Entities.BranchBranding>(
                brandings),
            new TestCurrentUser());

    private static BrandingLayoutSettings Layout() =>
        new()
        {
            MainColor = "#0070C4",
            SecondaryColor = "#FFFFFF",
            BackgroundColor = "#FFFFFF",
            HeaderColor = "#112233",
            ShowLanguagePage = true,
            LanguageButtonWidth = 40m,
            LanguageButtonText = "اختيار اللغة",
            ServiceButtonFontSize = 1.8m,
            FooterButtonText = "رجوع"
        };
}
