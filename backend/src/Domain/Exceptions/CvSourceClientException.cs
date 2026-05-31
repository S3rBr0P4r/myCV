namespace Backend.Domain.Exceptions;

public sealed class CvSourceClientException : Exception
{
    public CvSourceClientException()
        : base("CV data source is currently unavailable. Please try again later.")
    {
    }
}
