using QControl.Domain.Entities;

namespace QControl.Application.Tests.BranchDisplays;

public sealed class BranchDisplayDomainTests
{
    [Fact]
    public void Configuration_normalizes_colors_and_titles_without_device_or_logo_fields()
    {
        var settings = QControl.Application.Tests.TestSupport.EntityTestFactory
            .BranchDisplayConfigurationSettings() with
        {
            DisplayBackgroundColor = "  #aabbcc  ",
            MainTitleEn = "  Main display  "
        };

        var configuration = BranchDisplayConfiguration.Create(
            4,
            settings,
            QControl.Application.Tests.TestSupport.EntityTestFactory.CurrentUserId);

        Assert.Equal("#AABBCC", configuration.DisplayBackgroundColor);
        Assert.Equal("Main display", configuration.MainTitleEn);
        Assert.Null(typeof(BranchDisplayConfiguration).GetProperty("LogoPath"));
        Assert.Null(typeof(BranchDisplayConfiguration).GetProperty("DisplayId"));
        Assert.DoesNotContain(
            typeof(BranchDisplayConfiguration).GetProperties(),
            property => property.Name.Contains("Highlight", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Message_lifecycle_is_reversible_and_never_deletes_the_entity()
    {
        var message = BranchDisplayMessage.Create(
            2,
            "  رسالة  ",
            "  Message  ",
            1,
            QControl.Application.Tests.TestSupport.EntityTestFactory.CurrentUserId);

        Assert.True(message.IsActive);
        Assert.Equal("رسالة", message.TextAr);
        Assert.True(message.Deactivate(DateTime.UtcNow, QControl.Application.Tests.TestSupport.EntityTestFactory.CurrentUserId));
        Assert.False(message.IsActive);
        Assert.False(message.Deactivate(DateTime.UtcNow, QControl.Application.Tests.TestSupport.EntityTestFactory.CurrentUserId));
        Assert.True(message.Reactivate(DateTime.UtcNow, QControl.Application.Tests.TestSupport.EntityTestFactory.CurrentUserId));
        Assert.True(message.IsActive);
    }
}
