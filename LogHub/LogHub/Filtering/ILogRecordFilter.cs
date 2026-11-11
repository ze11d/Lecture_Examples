using LogHub.Models;

namespace LogHub.Filtering;

public interface ILogRecordFilter
{
    ILogRecordFilter SetNext(ILogRecordFilter next);

    bool IsAllowed(LogRecord record);
}