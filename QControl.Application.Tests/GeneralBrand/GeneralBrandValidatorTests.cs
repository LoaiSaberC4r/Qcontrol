using Qcontrol.Application.Features.GeneralBrand.Command.CreateGeneralBrand;
using Qcontrol.Application.Features.GeneralBrand.Command.UpdateGeneralBrand;

namespace QControl.Application.Tests.GeneralBranding;

public sealed class GeneralBrandValidatorTests
{
    [Fact]
    public void Create_accepts_complete_valid_layout()
    {
        var result = new CreateGeneralBrandCommandValidator()
            .Validate(GeneralBrandTestData.CreateCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_rejects_invalid_color_and_dimension()
    {
        var command = GeneralBrandTestData.CreateCommand() with
        {
            HeaderColor = "#FFF",
            ServiceButtonWidth = 0
        };

        var result = new CreateGeneralBrandCommandValidator()
            .Validate(command);

        Assert.Contains(result.Errors, x =>
            x.PropertyName == nameof(command.HeaderColor));
        Assert.Contains(result.Errors, x =>
            x.PropertyName == nameof(command.ServiceButtonWidth));
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("AQID")]
    public void Update_rejects_missing_or_malformed_row_version(string value)
    {
        var result = new UpdateGeneralBrandCommandValidator()
            .Validate(GeneralBrandTestData.UpdateCommand() with
            {
                RowVersion = value
            });

        Assert.Contains(result.Errors, x =>
            x.PropertyName == nameof(UpdateGeneralBrandCommand.RowVersion));
    }
}
