using BuildingBlock.Domain.EntitiesHelper;

namespace QControl.Domain.Entities;

public sealed class ServiceGlobalizationRequestItem : Entity<int>
{
    private ServiceGlobalizationRequestItem()
    {
    }

    public int RequestId { get; private set; }

    public ServiceGlobalizationRequest Request { get; private set; } = null!;

    public int ServiceId { get; private set; }

    public Service Service { get; private set; } = null!;

    public static ServiceGlobalizationRequestItem Create(int serviceId)
    {
        return new ServiceGlobalizationRequestItem
        {
            ServiceId = serviceId
        };
    }

    public static ServiceGlobalizationRequestItem Create(Service service)
    {
        return new ServiceGlobalizationRequestItem
        {
            Service = service
        };
    }
}
