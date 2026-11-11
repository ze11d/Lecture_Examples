using LogHub.Models;

namespace LogHub.Observers;

public class ConsoleLogObserver : ILogPipelineObserver
{
    public void OnRecordProcessed(LogRecord record)
    {
        Console.WriteLine(
            $"[{record.Date}] {record.IpAddress} {record.Request} {record.StatusCode}");
    }
}