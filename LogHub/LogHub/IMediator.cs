namespace LogHub;

public interface IMediator
{
    bool IsOverloaded { get; }

    void Notify(object sender, string eventName);

    void WaitIfOverloaded();
}