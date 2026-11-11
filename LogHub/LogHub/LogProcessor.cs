using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Importing;
using LogHub.Sinks;

namespace LogHub;

public class LogProcessor
{
    private readonly ILogImporter _importer;
    private readonly SeverityFilter _filter;
    private readonly PlainTextFormatter _formatter;
    private readonly IEnumerable<ILogSink> _sinks;

    public LogProcessor(
        ILogImporter importer,
        SeverityFilter filter,
        PlainTextFormatter formatter,
        IEnumerable<ILogSink> sinks)
    {
        _importer = importer;
        _filter = filter;
        _formatter = formatter;
        _sinks = sinks;
    }

    public (int Written, int Skipped) Process(string source)
    {
        int written = 0;
        int skipped = 0;

        foreach (var record in _importer.Import(source))
        {
            if (!_filter.IsAllowed(record))
            {
                skipped++;
                continue;
            }

            foreach (var sink in _sinks)
            {
                sink.Write(record);
            }

            written++;
        }

        return (written, skipped);
    }
}