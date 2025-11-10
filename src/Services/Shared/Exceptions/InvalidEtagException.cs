namespace BuildingBlocks.Exceptions;

public class InvalidEtagException : Exception
{
    public InvalidEtagException(string message) : base(message)
    {
    }

    public InvalidEtagException(int etag) : base($"Invalid etag \"{etag}\" ")
    {
    }
}