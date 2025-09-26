namespace WExpert.Helpers.Exceptions;

public class UnexpactedException : Exception
{
    public UnexpactedException()
    {
    }

    public UnexpactedException(string message) : base(message)
    {
    }

    public UnexpactedException(string message, Exception inner) : base(message, inner)
    {
    }
}