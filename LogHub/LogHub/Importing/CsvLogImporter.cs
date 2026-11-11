using System.Globalization;
using LogHub.Models;

namespace LogHub.Importing;

public class CsvLogImporter : LogImporterBase
{
    private string[] _lines = Array.Empty<string>();

    protected override void Open(string source)
    {
        if (!File.Exists(source))
            throw new FileNotFoundException("CSV-файл не найден.", source);
    }

    protected override IEnumerable<string> Read(string source)
    {
        _lines = File.ReadAllLines(source);

        // Первая строка — заголовок.
        return _lines.Skip(1);
    }

    protected override LogRecord? Parse(string line)
    {
        var parts = line.Split(',');

        if (parts.Length < 8)
            return null;

        if (!DateTime.TryParse(
                parts[3],
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
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
        _lines = Array.Empty<string>();
    }

    protected override void Dispose()
    {
        // Дополнительные ресурсы не используются.
    }
}