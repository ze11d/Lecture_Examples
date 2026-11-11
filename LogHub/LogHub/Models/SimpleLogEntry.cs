namespace LogHub.Models;

public class SimpleLogEntry : LogEntry
{
    public string Message { get; set; } = string.Empty;

    public override void Accept(ILogEntryVisitor visitor)
    {
        visitor.Visit(this);
    }
}