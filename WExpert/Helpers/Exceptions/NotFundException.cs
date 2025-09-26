namespace WExpert.Helpers.Exceptions;

public class NotFundException : Exception
{
    public NotFundException()
    {
    }

    public NotFundException(string message) : base(message)
    {
    }

    public NotFundException(string message, Exception inner) : base(message, inner)
    {
    }
}