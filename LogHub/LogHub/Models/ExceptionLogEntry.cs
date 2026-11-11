namespace LogHub.Models;

public class ExceptionLogEntry : LogEntry
{
    public string Message { get; set; } = string.Empty;

    public string StackTrace { get; set; } = string.Empty;

    public override void Accept(ILogEntryVisitor visitor)
    {
        visitor.Visit(this);
    }
}