namespace QControl.Application.Abstraction.Presistence;

public interface IServicePermanentDeleteRepository
{
    Task DeletePermanentlyAsync(
        int serviceId,
        byte[] rowVersion,
        CancellationToken cancellationToken);
}
