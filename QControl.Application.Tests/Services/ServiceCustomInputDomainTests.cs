using QControl.Application.Tests.TestSupport;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.Services;

public sealed class ServiceCustomInputDomainTests
{
    [Fact]
    public void String_input_normalizes_text_and_clears_integer_restrictions()
    {
        var service = EntityTestFactory.Service(1);

        var input = service.AddCustomInput(
            " NationalId ",
            " National ID ",
            "   ",
            ServiceCustomInputType.String,
            true,
            14,
            14,
            1,
            9,
            " 2 ",
            1,
            EntityTestFactory.CurrentUserId);

        Assert.Equal("NationalId", input.Name);
        Assert.Equal("National ID", input.LabelEn);
        Assert.Null(input.LabelAr);
        Assert.Equal("2", input.StartWith);
        Assert.Null(input.MinValue);
        Assert.Null(input.MaxValue);
    }

    [Fact]
    public void Integer_input_clears_string_restrictions()
    {
        var service = EntityTestFactory.Service(1);

        var input = service.AddCustomInput(
            "Age",
            null,
            null,
            ServiceCustomInputType.Integer,
            false,
            1,
            3,
            18,
            100,
            "1",
            1,
            EntityTestFactory.CurrentUserId);

        Assert.Null(input.MinLength);
        Assert.Null(input.MaxLength);
        Assert.Null(input.StartWith);
        Assert.Equal(18, input.MinValue);
        Assert.Equal(100, input.MaxValue);
    }

    [Fact]
    public void Aggregate_rejects_custom_input_from_another_service()
    {
        var first = EntityTestFactory.Service(1);
        var second = EntityTestFactory.Service(2);
        var input = EntityTestFactory.ServiceCustomInput(first, 10, "Input");

        Assert.Throws<InvalidOperationException>(() =>
            second.DeactivateCustomInput(
                input,
                EntityTestFactory.CurrentUserId));
    }
}
