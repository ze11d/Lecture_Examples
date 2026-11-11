using LogHub.Models;

namespace LogHub.Filtering;

public class RecordAgeFilter : LogRecordFilterBase
{
    private readonly TimeSpan _maximumAge;

    public RecordAgeFilter(TimeSpan maximumAge)
    {
        _maximumAge = maximumAge;
    }

    protected override bool IsMatch(LogRecord record)
    {
        var age = DateTime.UtcNow - record.Date.ToUniversalTime();
        return age <= _maximumAge;
    }
}