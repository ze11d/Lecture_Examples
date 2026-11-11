using LogHub.Configuration;
using LogHub.Models;

namespace LogHub.Filtering;

public class LogFilterChainFactory
{
    private readonly IConfigService _config;

    public LogFilterChainFactory(IConfigService config)
    {
        _config = config;
    }

    public LogFilterChain Create()
    {
        var order = _config.GetValue("FilterOrder");

        if (string.IsNullOrWhiteSpace(order))
            return new LogFilterChain(Array.Empty<ILogRecordFilter>());

        var filters = new Dictionary<string, Func<ILogRecordFilter>>
        {
            ["severity"] = () => new SeverityRecordFilter(LogSeverity.Info),
            ["age"] = () => new RecordAgeFilter(TimeSpan.FromDays(7)),
            ["duplicates"] = () => new DuplicateRecordFilter()
        };

        var chain = order.Split(',', StringSplitOptions.TrimEntries)
            .Select(name =>
                filters.TryGetValue(name, out var create)
                    ? create()
                    : throw new InvalidOperationException(
                        $"Неизвестный фильтр: {name}"));

        return new LogFilterChain(chain);
    }
}