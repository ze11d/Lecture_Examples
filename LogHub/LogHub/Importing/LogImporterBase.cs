
using LogHub.Models;

namespace LogHub.Importing;

public abstract class LogImporterBase : ILogImporter
{
    public IEnumerable<LogRecord> Import(string source)
    {
        var records = new List<LogRecord>();

        Open(source);

        try
        {
            foreach (var line in Read(source))
            {
                var record = Parse(line);

                if (record != null && Validate(record))
                    records.Add(record);
            }
        }
        finally
        {
            try
            {
                Close();
            }
            finally
            {
                Dispose();
            }
        }

        return records;
    }

    protected virtual void Open(string source)
    {
    }

    protected abstract IEnumerable<string> Read(string source);

    protected abstract LogRecord? Parse(string line);

    protected virtual bool Validate(LogRecord record)
    {
        return true;
    }

    protected virtual void Close()
    {
    }

    protected virtual void Dispose()
    {
    }
}
