using LogHub.Models;

namespace LogHub.Filtering;

public class SeverityRecordFilter : LogRecordFilterBase
{
    private readonly LogSeverity _minimumSeverity;

    public SeverityRecordFilter(LogSeverity minimumSeverity)
    {
        _minimumSeverity = minimumSeverity;
    }

    protected override bool IsMatch(LogRecord record)
    {
        return record.Severity >= _minimumSeverity;
    }
}