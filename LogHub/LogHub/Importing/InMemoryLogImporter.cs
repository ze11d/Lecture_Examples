
using LogHub.Models;

namespace LogHub.Importing;

public class InMemoryLogImporter : LogImporterBase
{
    private readonly IEnumerable<LogRecord> _records;

    public InMemoryLogImporter(IEnumerable<LogRecord> records)
    {
        _records = records;
    }

    protected override void Open(string source)
    {
        // Источник уже находится в памяти.
    }

    protected override IEnumerable<string> Read(string source)
    {
        return _records.Select(record =>
            string.Join("|",
                record.IpAddress,
                record.UserIdentifier,
                record.UserName,
                record.Date.ToString("O"),
                record.Request,
                record.StatusCode,
                record.ResponseSize,
                record.UserAgent));
    }

    protected override LogRecord? Parse(string line)
    {
        var parts = line.Split('|');

        if (parts.Length != 8)
            return null;

        if (!DateTime.TryParse(parts[3], out var date))
            return null;

        if (!int.TryParse(parts[5], out var statusCode))
            return null;

        if (!long.TryParse(parts[6], out var responseSize))
            return null;

        return new LogRecord
        {
            IpAddress = parts[0],
            UserIdentifier = parts[1],
            UserName = parts[2],
            Date = date,
            Request = parts[4],
            StatusCode = statusCode,
            ResponseSize = responseSize,
            UserAgent = parts[7]
        };
    }

    protected override void Close()
    {
    }

    protected override void Dispose()
    {
    }
}
