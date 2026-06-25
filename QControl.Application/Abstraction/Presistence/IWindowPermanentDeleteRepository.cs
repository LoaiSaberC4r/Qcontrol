namespace QControl.Application.Abstraction.Presistence;

public interface IWindowPermanentDeleteRepository
{
    Task<int> DeletePermanentlyAsync(
        int windowId,
        CancellationToken cancellationToken);
}
