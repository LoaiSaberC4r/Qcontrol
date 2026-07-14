using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class ServiceWorkflow : AggregateRoot<int>
{
    private readonly List<ServiceWorkflowStep> _steps = new();

    private ServiceWorkflow()
    {
    }

    public string ArabicName { get; private set; } = string.Empty;

    public string EnglishName { get; private set; } = string.Empty;

    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public int LeafServiceId { get; private set; }

    public Service LeafService { get; private set; } = null!;

    public bool IsDefault { get; private set; }

    public bool IsActive { get; private set; } = true;

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    public Guid? DeactivatedByApplicationUserId { get; private set; }

    public ApplicationUser? DeactivatedByApplicationUser { get; private set; }

    public DateTime? DeactivatedOnUtc { get; private set; }

    public Guid? ReactivatedByApplicationUserId { get; private set; }

    public ApplicationUser? ReactivatedByApplicationUser { get; private set; }

    public DateTime? ReactivatedOnUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public IReadOnlyCollection<ServiceWorkflowStep> Steps =>
        _steps.AsReadOnly();

    public static ServiceWorkflow Create(
        int branchId,
        int leafServiceId,
        string arabicName,
        string englishName,
        bool isDefault,
        IEnumerable<ServiceWorkflowStepData> steps,
        Guid createdByApplicationUserId)
    {
        var workflow = new ServiceWorkflow
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId,
            ArabicName = NormalizeRequired(arabicName),
            EnglishName = NormalizeRequired(englishName),
            IsActive = true,
            IsDefault = isDefault,
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };

        workflow.ReplaceSteps(steps);

        return workflow;
    }

    public void Update(
        string arabicName,
        string englishName,
        IEnumerable<ServiceWorkflowStepData> steps,
        Guid lastModifiedByApplicationUserId)
    {
        ArabicName = NormalizeRequired(arabicName);
        EnglishName = NormalizeRequired(englishName);
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;

        ReplaceSteps(steps);
    }

    public void Deactivate(
        DateTime deactivatedOnUtc,
        Guid deactivatedByApplicationUserId)
    {
        IsActive = false;
        DeactivatedOnUtc = deactivatedOnUtc;
        DeactivatedByApplicationUserId = deactivatedByApplicationUserId;
        LastModifiedByApplicationUserId = deactivatedByApplicationUserId;
    }

    public void Reactivate(
        DateTime reactivatedOnUtc,
        Guid reactivatedByApplicationUserId)
    {
        IsActive = true;
        ReactivatedOnUtc = reactivatedOnUtc;
        ReactivatedByApplicationUserId = reactivatedByApplicationUserId;
        LastModifiedByApplicationUserId = reactivatedByApplicationUserId;
    }

    public void MarkAsDefault(Guid modifiedByApplicationUserId)
    {
        IsDefault = true;
        LastModifiedByApplicationUserId = modifiedByApplicationUserId;
    }

    public void RemoveDefault(Guid modifiedByApplicationUserId)
    {
        IsDefault = false;
        LastModifiedByApplicationUserId = modifiedByApplicationUserId;
    }

    private void ReplaceSteps(IEnumerable<ServiceWorkflowStepData> steps)
    {
        _steps.Clear();

        foreach (var step in steps.OrderBy(x => x.StepOrder))
        {
            _steps.Add(ServiceWorkflowStep.Create(
                step.ServiceId,
                step.StepOrder));
        }
    }

    private static string NormalizeRequired(string value) => value.Trim();
}

public sealed record ServiceWorkflowStepData(
    int ServiceId,
    int StepOrder);
