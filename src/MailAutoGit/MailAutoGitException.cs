using System.Runtime.Serialization;

namespace MailAutoGit;

public class MailAutoGitException : Exception
{
    public MailAutoGitException()
    {
    }

    public MailAutoGitException(string? message) : base(message)
    {
    }

    public MailAutoGitException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected MailAutoGitException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}