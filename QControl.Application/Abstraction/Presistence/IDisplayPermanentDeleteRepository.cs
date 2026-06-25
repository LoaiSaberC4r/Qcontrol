namespace QControl.Application.Abstraction.Presistence;

public interface IDisplayPermanentDeleteRepository
{
    Task<int> DeletePermanentlyAsync(
        int displayId,
        CancellationToken cancellationToken);
}
