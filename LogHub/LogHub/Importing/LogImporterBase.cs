using LogHub.Models;
using LogHub;

namespace LogHub.Importing;

public abstract class LogImporterBase : ILogImporter, IMediatorAwareImporter
{
    private IMediator? _mediator;

    public void SetMediator(IMediator mediator)
    {
       _mediator = mediator;
    }   
    
    public IEnumerable<LogRecord> Import(string source)
    {
        Open(source);

        try
        {
            foreach (var line in Read(source))
            {
                _mediator?.WaitIfOverloaded();

                var record = Parse(line);

                if (record != null && Validate(record))
                    yield return record;
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
