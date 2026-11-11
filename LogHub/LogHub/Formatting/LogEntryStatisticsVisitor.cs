using LogHub.Models;

namespace LogHub.Formatting;

public class LogEntryStatisticsVisitor : ILogEntryVisitor
{
    public int SimpleCount { get; private set; }

    public int ExceptionCount { get; private set; }

    public int MetricCount { get; private set; }

    public void Visit(SimpleLogEntry entry)
    {
        SimpleCount++;
    }

    public void Visit(ExceptionLogEntry entry)
    {
        ExceptionCount++;
    }

    public void Visit(MetricLogEntry entry)
    {
        MetricCount++;
    }
}