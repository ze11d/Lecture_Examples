using LogHub.Models;

namespace LogHub.Formatting;

public class LogEntryTextVisitor : ILogEntryVisitor
{
    public string Result { get; private set; } = string.Empty;

    public void Visit(SimpleLogEntry entry)
    {
        Result = $"Log: {entry.Message}";
    }

    public void Visit(ExceptionLogEntry entry)
    {
        Result = $"Exception: {entry.Message}\nStack trace: {entry.StackTrace}";
    }

    public void Visit(MetricLogEntry entry)
    {
        Result = $"Metric: {entry.Value}";
    }
}