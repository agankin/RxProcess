namespace RxProcess;

internal class RxSubscription : IDisposable
{
    private readonly IObserver<StdOutLine> _observer;
    private readonly RxSubscriptionSet _subscriptions;

    private volatile int _disposed;

    public RxSubscription(IObserver<StdOutLine> observer, RxSubscriptionSet subscriptions)
    {
        _observer = observer;
        _subscriptions = subscriptions;
    }

    internal void OnNext(StdOutLine line)
    {
        try
        {
            _observer.OnNext(line);
        }
        catch {}
    }

    internal void Complete()
    {
        try
        {
            _observer.OnCompleted();
        }
        catch {}
        
        Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        var disposed = Interlocked.CompareExchange(ref _disposed, 1, 0);
        if (disposed != 0)
            return;

        _subscriptions.Remove(this);
    }
}