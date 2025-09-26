namespace WExpert.Helpers.Exceptions;

public class TooManyRequestException : Exception
{
    public TooManyRequestException()
    {
    }

    public TooManyRequestException(string message) : base(message)
    {
    }

    public TooManyRequestException(string message, Exception inner) : base(message, inner)
    {
    }
}