using System.Runtime.Serialization;

namespace GFA;

public class GFAException : Exception
{
    public GFAException()
    {
    }

    public GFAException(string? message) : base(message)
    {
    }

    public GFAException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected GFAException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}