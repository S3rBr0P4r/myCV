namespace Backend.Domain.Exceptions;

public sealed class CvSourceException : Exception
{
    public CvSourceException(string message)
        : base(message)
    {
    }

    public CvSourceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
