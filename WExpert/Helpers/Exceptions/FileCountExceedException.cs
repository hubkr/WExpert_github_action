namespace WExpert.Helpers.Exceptions;

public class FileCountExceedException : Exception
{
    public FileCountExceedException()
    {
    }

    public FileCountExceedException(string message) : base(message)
    {
    }

    public FileCountExceedException(string message, Exception inner) : base(message, inner)
    {
    }
}