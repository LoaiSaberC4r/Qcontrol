namespace QControl.Application.Abstraction.Presistence;

public sealed class WindowPermanentDeleteConflictException : Exception
{
    public WindowPermanentDeleteConflictException(Exception innerException)
        : base(
            "The window cannot be permanently deleted because related records exist.",
            innerException)
    {
    }
}
