namespace ConsoleDemo.VismaConnect;

public class Disposable : IDisposable
{
    private readonly Action action;
    public Disposable(Action action)
    {
        this.action = action;
    }
    public void Dispose()
    {
        action?.Invoke();
        GC.SuppressFinalize(this);
    }
}
