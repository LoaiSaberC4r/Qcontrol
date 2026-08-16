using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;
using QControl.Domain.Enums;

namespace QControl.Domain.Entities;

public sealed class ServiceCustomInput : Entity<int>
{
    private ServiceCustomInput()
    {
    }

    public int ServiceId { get; private set; }

    public Service Service { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string? LabelEn { get; private set; }

    public string? LabelAr { get; private set; }

    public ServiceCustomInputType Type { get; private set; }

    public bool IsRequired { get; private set; }

    public int? MinLength { get; private set; }

    public int? MaxLength { get; private set; }

    public int? MinValue { get; private set; }

    public int? MaxValue { get; private set; }

    public string? StartWith { get; private set; }

    public int Order { get; private set; }

    public bool IsActive { get; private set; } = true;

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    internal static ServiceCustomInput Create(
        Service service,
        string name,
        string? labelEn,
        string? labelAr,
        ServiceCustomInputType type,
        bool isRequired,
        int? minLength,
        int? maxLength,
        int? minValue,
        int? maxValue,
        string? startWith,
        int order,
        Guid createdByApplicationUserId)
    {
        ArgumentNullException.ThrowIfNull(service);

        var customInput = new ServiceCustomInput
        {
            Service = service,
            ServiceId = service.Id,
            CreatedByApplicationUserId = createdByApplicationUserId,
            IsActive = true
        };

        customInput.ApplyDefinition(
            name,
            labelEn,
            labelAr,
            type,
            isRequired,
            minLength,
            maxLength,
            minValue,
            maxValue,
            startWith,
            order);

        return customInput;
    }

    internal void Update(
        string name,
        string? labelEn,
        string? labelAr,
        ServiceCustomInputType type,
        bool isRequired,
        int? minLength,
        int? maxLength,
        int? minValue,
        int? maxValue,
        string? startWith,
        int order,
        Guid lastModifiedByApplicationUserId)
    {
        ApplyDefinition(
            name,
            labelEn,
            labelAr,
            type,
            isRequired,
            minLength,
            maxLength,
            minValue,
            maxValue,
            startWith,
            order);
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    internal void Deactivate(Guid lastModifiedByApplicationUserId)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    internal void Restore(Guid lastModifiedByApplicationUserId)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    private void ApplyDefinition(
        string name,
        string? labelEn,
        string? labelAr,
        ServiceCustomInputType type,
        bool isRequired,
        int? minLength,
        int? maxLength,
        int? minValue,
        int? maxValue,
        string? startWith,
        int order)
    {
        Name = NormalizeRequired(name);
        LabelEn = NormalizeOptional(labelEn);
        LabelAr = NormalizeOptional(labelAr);
        Type = type;
        IsRequired = isRequired;
        Order = order;

        if (type == ServiceCustomInputType.String)
        {
            MinLength = minLength;
            MaxLength = maxLength;
            StartWith = NormalizeOptional(startWith);
            MinValue = null;
            MaxValue = null;
            return;
        }

        MinLength = null;
        MaxLength = null;
        StartWith = null;
        MinValue = minValue;
        MaxValue = maxValue;
    }

    private static string NormalizeRequired(string value) => value.Trim();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
