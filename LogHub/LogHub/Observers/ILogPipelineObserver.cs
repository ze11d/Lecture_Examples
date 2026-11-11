using LogHub.Models;

namespace LogHub.Observers;

public interface ILogPipelineObserver
{
    void OnRecordProcessed(LogRecord record);
}