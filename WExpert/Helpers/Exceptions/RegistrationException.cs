namespace WExpert.Helpers.Exceptions;

public class RegistrationException : Exception
{
    public RegistrationException()
    {
    }

    public RegistrationException(string message) : base(message)
    {
    }

    public RegistrationException(string message, Exception inner) : base(message, inner)
    {
    }
}