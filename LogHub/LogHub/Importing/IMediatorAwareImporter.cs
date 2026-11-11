namespace LogHub.Importing;

public interface IMediatorAwareImporter
{
    void SetMediator(IMediator mediator);
}