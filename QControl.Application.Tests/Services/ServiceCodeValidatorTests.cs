using Qcontrol.Application.Features.Services.Command.CreateService;
using Qcontrol.Application.Features.Services.Command.UpdateService;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Services;

public sealed class ServiceCodeValidatorTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public void Create_and_update_accept_required_service_code()
    {
        var create = new CreateServiceCommandValidator().Validate(
            ValidCreate() with
            {
                ServiceCode = "MED-001",
                IsServiceCodeRequired = true
            });
        var update = new UpdateServiceCommandValidator().Validate(
            ValidUpdate() with
            {
                ServiceCode = "MED-001",
                IsServiceCodeRequired = true
            });

        Assert.True(create.IsValid);
        Assert.True(update.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_and_update_reject_missing_required_service_code(
        string? serviceCode)
    {
        var create = new CreateServiceCommandValidator().Validate(
            ValidCreate() with
            {
                ServiceCode = serviceCode,
                IsServiceCodeRequired = true
            });
        var update = new UpdateServiceCommandValidator().Validate(
            ValidUpdate() with
            {
                ServiceCode = serviceCode,
                IsServiceCodeRequired = true
            });

        Assert.False(create.IsValid);
        Assert.False(update.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_and_update_accept_blank_optional_service_code(
        string? serviceCode)
    {
        var create = new CreateServiceCommandValidator().Validate(
            ValidCreate() with
            {
                ServiceCode = serviceCode,
                IsServiceCodeRequired = false
            });
        var update = new UpdateServiceCommandValidator().Validate(
            ValidUpdate() with
            {
                ServiceCode = serviceCode,
                IsServiceCodeRequired = false
            });

        Assert.True(create.IsValid);
        Assert.True(update.IsValid);
    }

    [Fact]
    public void Create_and_update_reject_code_when_not_required()
    {
        var create = new CreateServiceCommandValidator().Validate(
            ValidCreate() with
            {
                ServiceCode = "MED-001",
                IsServiceCodeRequired = false
            });
        var update = new UpdateServiceCommandValidator().Validate(
            ValidUpdate() with
            {
                ServiceCode = "MED-001",
                IsServiceCodeRequired = false
            });

        Assert.False(create.IsValid);
        Assert.False(update.IsValid);
    }

    [Fact]
    public void Create_and_update_reject_code_over_approved_maximum_length()
    {
        var serviceCode = new string(
            'A',
            ServiceCodeNormalizer.MaxLength + 1);
        var create = new CreateServiceCommandValidator().Validate(
            ValidCreate() with
            {
                ServiceCode = serviceCode,
                IsServiceCodeRequired = true
            });
        var update = new UpdateServiceCommandValidator().Validate(
            ValidUpdate() with
            {
                ServiceCode = serviceCode,
                IsServiceCodeRequired = true
            });

        Assert.False(create.IsValid);
        Assert.False(update.IsValid);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("MED-001", false)]
    public void Domain_creation_guards_against_inconsistent_service_code_state(
        string? serviceCode,
        bool isServiceCodeRequired)
    {
        Assert.Throws<ArgumentException>(() =>
            EntityTestFactory.Service(
                1,
                serviceCode: serviceCode,
                isServiceCodeRequired: isServiceCodeRequired));
    }

    private static CreateServiceCommand ValidCreate()
        => new()
        {
            ArabicName = "Arabic Service",
            EnglishName = "English Service",
            IsTicketIssuable = false,
            OrderNo = 1,
            Priority = 1
        };

    private static UpdateServiceCommand ValidUpdate()
        => new()
        {
            Id = 1,
            ArabicName = "Arabic Service",
            EnglishName = "English Service",
            IsTicketIssuable = false,
            OrderNo = 1,
            Priority = 1,
            RowVersion = ValidRowVersion
        };
}
