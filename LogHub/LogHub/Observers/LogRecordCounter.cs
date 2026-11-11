using LogHub.Models;

namespace LogHub.Observers;

public class LogRecordCounter : ILogPipelineObserver
{
    public int Count { get; private set; }

    public void OnRecordProcessed(LogRecord record)
    {
        Count++;
    }
}