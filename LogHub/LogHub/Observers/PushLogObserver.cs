using LogHub.Models;

namespace LogHub.Observers;

public class PushLogObserver
{
    public int Count { get; private set; }

    public LogRecord? LastRecord { get; private set; }

    public void OnRecordProcessed(LogRecord record)
    {
        LastRecord = record;
        Count++;
    }
}