using LogHub.Models;

namespace LogHub.Observers;

public class CriticalAlertObserver : ILogPipelineObserver
{
    public void OnRecordProcessed(LogRecord record)
    {
        if (record.Severity == LogSeverity.Critical)
        {
            Console.WriteLine($"CRITICAL ALERT: {record.Request}");
        }
    }
}