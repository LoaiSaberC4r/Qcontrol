using BuildingBlock.Domain.Enums;
using Qcontrol.Application.Features.GeneralBrand.Query.GetGeneralBrand;
using QControl.Application.Tests.TestSupport;

namespace QControl.Application.Tests.GeneralBranding;

public sealed class GetGeneralBrandQueryHandlerTests
{
    [Fact]
    public async Task Returns_complete_configuration_using_projection()
    {
        var generalBrand = EntityTestFactory.GeneralBrand(
            1,
            GeneralBrandTestData.Layout(),
            new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        var repository =
            new InMemoryWriteReadRepository<QControl.Domain.Entities.GeneralBrand>(
                new List<QControl.Domain.Entities.GeneralBrand>
                {
                    generalBrand
                });
        var handler = new GetGeneralBrandQueryHandler(
            repository,
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetGeneralBrandQuery(),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsConfigured);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("#0070C4", result.Value.MainColor);
        Assert.Equal("اختيار اللغة", result.Value.LanguageButtonText);
        Assert.Equal(1.8m, result.Value.ServiceButtonFontSize);
        Assert.Equal("AQIDBAUGBwg=", result.Value.RowVersion);
        Assert.Equal(1, repository.FirstOrDefaultProjectionSpecCallCount);

        var spec = new GetGeneralBrandSpec();
        Assert.Equal(TrackingBehavior.NoTracking, spec.Tracking);
        Assert.Null(typeof(Qcontrol.Application.Features.GeneralBrand.Shared
            .GeneralBrandResponse).GetProperty("SingletonKey"));
    }

    [Fact]
    public async Task Returns_not_configured_when_missing()
    {
        var handler = new GetGeneralBrandQueryHandler(
            new InMemoryWriteReadRepository<QControl.Domain.Entities.GeneralBrand>(
                new List<QControl.Domain.Entities.GeneralBrand>()),
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetGeneralBrandQuery(),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsConfigured);
        Assert.Null(result.Value.Id);
        Assert.Null(result.Value.RowVersion);
    }
}
