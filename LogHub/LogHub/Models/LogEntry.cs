namespace LogHub.Models;

public abstract class LogEntry
{
    public abstract void Accept(ILogEntryVisitor visitor);
}