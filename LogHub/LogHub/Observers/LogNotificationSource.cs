using LogHub.Models;

namespace LogHub.Observers;

public class LogNotificationSource
{
    private LogRecord? _currentRecord;

    public event Action? RecordProcessed;

    public void NotifyPull(LogRecord record)
    {
        _currentRecord = record;
        RecordProcessed?.Invoke();
    }

    public LogRecord? GetRecord()
    {
        return _currentRecord;
    }

    public void NotifyPush(
        LogRecord record,
        PushLogObserver observer)
    {
        observer.OnRecordProcessed(record);
    }
}