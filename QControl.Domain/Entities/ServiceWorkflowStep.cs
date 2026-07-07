namespace QControl.Domain.Entities;

public sealed class ServiceWorkflowStep
{
    private ServiceWorkflowStep()
    {
    }

    public int Id { get; private set; }

    public int ServiceWorkflowId { get; private set; }

    public ServiceWorkflow ServiceWorkflow { get; private set; } = null!;

    public int ServiceId { get; private set; }

    public Service Service { get; private set; } = null!;

    public int StepOrder { get; private set; }

    public static ServiceWorkflowStep Create(
        int serviceId,
        int stepOrder)
    {
        return new ServiceWorkflowStep
        {
            ServiceId = serviceId,
            StepOrder = stepOrder
        };
    }
}
