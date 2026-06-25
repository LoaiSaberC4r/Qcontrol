namespace QControl.Application.Abstraction.Presistence;

public interface ITerminalPermanentDeleteRepository
{
    Task<int> DeletePermanentlyAsync(
        int terminalId,
        CancellationToken cancellationToken);
}
