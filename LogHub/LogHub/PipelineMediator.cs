namespace LogHub;

public class PipelineMediator : IMediator, IDisposable
{
    private readonly ManualResetEventSlim _ready = new(true);

    public bool IsOverloaded => !_ready.IsSet;

    public void Notify(object sender, string eventName)
    {
        if (eventName == "Overloaded")
            _ready.Reset();

        if (eventName == "Ready")
            _ready.Set();
    }

    public void WaitIfOverloaded()
    {
        _ready.Wait();
    }

    public void Dispose()
    {
        _ready.Dispose();
    }
}