using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.Services.Shared;

public abstract class ServiceCustomInputDefinitionCommand
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

public sealed class CreateServiceCustomInputCommand
    : ServiceCustomInputDefinitionCommand;

public sealed class UpdateServiceCustomInputCommand
    : ServiceCustomInputDefinitionCommand
{
    public int? CustomInputId { get; init; }
}

public sealed class ServiceCustomInputResponse
{
    public int CustomInputId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? LabelEn { get; init; }

    public string? LabelAr { get; init; }

    public ServiceCustomInputType Type { get; init; }

    public string TypeName { get; init; } = string.Empty;

    public bool IsRequired { get; init; }

    public int? MinLength { get; init; }

    public int? MaxLength { get; init; }

    public int? MinValue { get; init; }

    public int? MaxValue { get; init; }

    public string? StartWith { get; init; }

    public int Order { get; init; }
}

internal sealed class ServiceCustomInputReadModel
{
    public int ServiceId { get; init; }

    public int CustomInputId { get; init; }

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

internal static class ServiceCustomInputResponseFactory
{
    public static ServiceCustomInputResponse FromReadModel(
        ServiceCustomInputReadModel item) => new()
        {
            CustomInputId = item.CustomInputId,
            Name = item.Name,
            LabelEn = item.LabelEn,
            LabelAr = item.LabelAr,
            Type = item.Type,
            TypeName = item.Type.ToString(),
            IsRequired = item.IsRequired,
            MinLength = item.MinLength,
            MaxLength = item.MaxLength,
            MinValue = item.MinValue,
            MaxValue = item.MaxValue,
            StartWith = item.StartWith,
            Order = item.Order
        };
}
