using LogHub.Models;

namespace LogHub.Filtering;

public class LogFilterChain
{
    private readonly ILogRecordFilter? _first;

    public LogFilterChain(IEnumerable<ILogRecordFilter> filters)
    {
        var filterList = filters.ToList();

        if (filterList.Count == 0)
            return;

        _first = filterList[0];

        for (int i = 0; i < filterList.Count - 1; i++)
        {
            filterList[i].SetNext(filterList[i + 1]);
        }
    }

    public bool IsAllowed(LogRecord record)
    {
        return _first?.IsAllowed(record) ?? true;
    }
}