namespace QControl.Application.Abstraction.Presistence;

public sealed class ServicePermanentDeleteConcurrencyException : Exception
{
    public ServicePermanentDeleteConcurrencyException()
        : base("The service changed before it could be permanently deleted.")
    {
    }
}
