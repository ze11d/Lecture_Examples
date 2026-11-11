using System.Collections.Concurrent;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Importing;
using LogHub.Sinks;
using LogHub.Observers;

namespace LogHub;

public class LogProcessor
{
    private readonly ILogImporter _importer;
    private readonly LogFilterChain _filterChain;
    private readonly PlainTextFormatter _formatter;
    private readonly IEnumerable<ILogSink> _sinks;
    private readonly LogPipelineSubject _subject;
    private readonly LogNotificationSource _notificationSource;
    private readonly PushLogObserver _pushObserver;
    private readonly IMediator _mediator;

    public LogProcessor(
        ILogImporter importer,
        LogFilterChain filterChain,
        PlainTextFormatter formatter,
        IEnumerable<ILogSink> sinks,
        LogPipelineSubject subject,
        LogNotificationSource notificationSource,
        PushLogObserver pushObserver,
        IMediator mediator)
    {
        _importer = importer;
        _filterChain = filterChain;
        _formatter = formatter;
        _sinks = sinks;
        _subject = subject;
        _notificationSource = notificationSource;
        _pushObserver = pushObserver;
        _mediator = mediator;
    }

    public (int Written, int Skipped) Process(string source)
    {
        using var queue = new BlockingCollection<LogHub.Models.LogRecord>(100);

        int written = 0;
        int skipped = 0;

        var consumer = Task.Run(() =>
        {
            foreach (var record in queue.GetConsumingEnumerable())
            {
                if (!_filterChain.IsAllowed(record))
                {
                    Interlocked.Increment(ref skipped);
                    continue;
                }

                foreach (var sink in _sinks)
                    sink.Write(record);

                _subject.Notify(record);
                _notificationSource.NotifyPush(record, _pushObserver);
                _notificationSource.NotifyPull(record);

                Interlocked.Increment(ref written);

                if (queue.Count < 100)
                    _mediator.Notify(this, "Ready");
            }
        });

        try
        {
            foreach (var record in _importer.Import(source))
            {
                queue.Add(record);

                if (queue.Count >= 100)
                    _mediator.Notify(this, "Overloaded");
            }
        }
        finally
        {
            queue.CompleteAdding();
        }

        consumer.GetAwaiter().GetResult();

        return (written, skipped);
    }
}
