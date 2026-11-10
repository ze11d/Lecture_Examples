using LogHub.Models;
using LogHub.Parsing;

namespace LogHub.Importing;

public class ApacheLogImporter : LogImporterBase
{
    private readonly ApacheLogParser _parser;
    private string[] _lines = Array.Empty<string>();

    public ApacheLogImporter(ApacheLogParser parser)
    {
        _parser = parser;
    }

    protected override void Open(string source)
    {
        if (!File.Exists(source))
            throw new FileNotFoundException("Файл логов не найден.", source);
    }

    protected override IEnumerable<string> Read(string source)
    {
        _lines = File.ReadAllLines(source);
        return _lines;
    }

    protected override LogRecord? Parse(string line)
    {
        return _parser.Parse(line);
    }

    protected override void Close()
    {
        _lines = Array.Empty<string>();
    }

    protected override void Dispose()
    {
        // Дополнительные ресурсы не используются.
    }

    protected override bool Validate(LogRecord record)
    {
        // Для Apache проверяем корректность IP-адреса.
        return System.Net.IPAddress.TryParse(record.IpAddress, out _);
    }
}