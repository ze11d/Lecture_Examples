using LogHub.Models;

namespace LogHub.Importing;

public interface ILogImporter
{
    IEnumerable<LogRecord> Import(string source);
}