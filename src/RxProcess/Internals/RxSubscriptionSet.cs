using System.Collections;
using System.Collections.Concurrent;

namespace RxProcessLib;

internal class RxSubscriptionSet : IEnumerable<RxSubscription>
{
    private readonly ConcurrentDictionary<RxSubscription, byte> _dictionary = new();

    internal RxSubscription Add(IObserver<StdOutLine> observer)
    {
        var subscription = new RxSubscription(observer, this);
        _dictionary.TryAdd(subscription, default);
        
        return subscription;
    }

    internal void Remove(RxSubscription subscription) => _dictionary.TryRemove(subscription, out _);
    
    /// <inheritdoc/>
    public IEnumerator<RxSubscription> GetEnumerator() => _dictionary.Keys.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}