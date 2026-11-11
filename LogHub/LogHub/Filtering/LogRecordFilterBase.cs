using LogHub.Models;

namespace LogHub.Filtering;

public abstract class LogRecordFilterBase : ILogRecordFilter
{
    private ILogRecordFilter? _next;

    public ILogRecordFilter SetNext(ILogRecordFilter next)
    {
        _next = next;
        return next;
    }

    public bool IsAllowed(LogRecord record)
    {
        return IsMatch(record) && (_next?.IsAllowed(record) ?? true);
    }

    protected abstract bool IsMatch(LogRecord record);
}