namespace RxProcess;

internal class DoDisposable : IDisposable
{
    private readonly Action _doAction;
    private volatile int _disposed;

    public DoDisposable(Action doAction)
    {
        _doAction = doAction;
    }

    public void Dispose()
    {
        var disposed = Interlocked.CompareExchange(ref _disposed, 1, 0);
        if (disposed != 0)
            return;

        _doAction();
    }
}