using LogHub.Models;

namespace LogHub.Filtering;

public class DuplicateRecordFilter : LogRecordFilterBase
{
    private readonly HashSet<string> _seen = new();

    protected override bool IsMatch(LogRecord record)
    {
        var key = $"{record.IpAddress}|{record.Date:O}|{record.Request}|{record.StatusCode}";

        return _seen.Add(key);
    }
}