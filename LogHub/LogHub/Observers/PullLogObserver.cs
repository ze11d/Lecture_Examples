using LogHub.Models;

namespace LogHub.Observers;

public class PullLogObserver
{
    private readonly Func<LogRecord?> _getRecord;

    public int Count { get; private set; }

    public LogRecord? LastRecord { get; private set; }

    public PullLogObserver(Func<LogRecord?> getRecord)
    {
        _getRecord = getRecord;
    }

    public void OnRecordProcessed()
    {
        LastRecord = _getRecord();
        Count++;
    }
}