using QControl.Domain.Enums;

namespace Qcontrol.Api.Contracts.Services;

public class CreateServiceCustomInputRequest
{
    public string Name { get; init; } = string.Empty;

    public string? LabelEn { get; init; }

    public string? LabelAr { get; init; }

    public ServiceCustomInputType Type { get; init; }

    public bool IsRequired { get; init; }

    public int? MinLength { get; init; }

    public int? MaxLength { get; init; }

    public int? MinValue { get; init; }

    public int? MaxValue { get; init; }

    public string? StartWith { get; init; }

    public int Order { get; init; }
}

public sealed class UpdateServiceCustomInputRequest
    : CreateServiceCustomInputRequest
{
    public int? CustomInputId { get; init; }
}
