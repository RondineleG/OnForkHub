namespace OnForkHub.Core.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public static void When(bool hasError, string message)
    {
        if (hasError)
            throw new DomainException(message);
    }
}

