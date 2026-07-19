namespace QControl.Application.Abstraction.Presistence;

public sealed class ServicePermanentDeleteConflictException : Exception
{
    public ServicePermanentDeleteConflictException(Exception innerException)
        : base(
            "The service cannot be permanently deleted because related records exist.",
            innerException)
    {
    }
}
