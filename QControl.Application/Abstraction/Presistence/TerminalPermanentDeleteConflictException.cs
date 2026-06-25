namespace QControl.Application.Abstraction.Presistence;

public sealed class TerminalPermanentDeleteConflictException : Exception
{
    public TerminalPermanentDeleteConflictException(Exception innerException)
        : base("Terminal permanent delete failed because related data exists.", innerException)
    {
    }
}
