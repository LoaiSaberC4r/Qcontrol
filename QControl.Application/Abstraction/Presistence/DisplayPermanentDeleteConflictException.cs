namespace QControl.Application.Abstraction.Presistence;

public sealed class DisplayPermanentDeleteConflictException : Exception
{
    public DisplayPermanentDeleteConflictException(Exception innerException)
        : base("Display permanent delete failed because related data exists.", innerException)
    {
    }
}
