using LogHub.Models;

namespace LogHub.Observers;

public class LogPipelineSubject
{
    private readonly List<ILogPipelineObserver> _observers = new();

    public void Subscribe(ILogPipelineObserver observer)
    {
        _observers.Add(observer);
    }

    public void Unsubscribe(ILogPipelineObserver observer)
    {
        _observers.Remove(observer);
    }

    public void Notify(LogRecord record)
    {
        foreach (var observer in _observers.ToList())
        {
            try
            {
                observer.OnRecordProcessed(record);
            }
            catch (Exception)
            {
                // Ошибка одного подписчика не мешает остальным.
            }
        }
    }
}