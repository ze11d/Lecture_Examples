namespace LogHub.Models;

public class MetricLogEntry : LogEntry
{
    public double Value { get; set; }

    public override void Accept(ILogEntryVisitor visitor)
    {
        visitor.Visit(this);
    }
}