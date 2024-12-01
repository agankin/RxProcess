using System.Collections;
using System.Collections.Concurrent;

namespace RxProcess;

internal class ConcurrentSet<T> : IEnumerable<T> where T : notnull
{
    private readonly ConcurrentDictionary<T, byte> _dictionary = new();

    public void Add(T item) => _dictionary.TryAdd(item, default);

    public void Remove(T item) => _dictionary.TryRemove(item, out _);
    
    public IEnumerator<T> GetEnumerator() => _dictionary.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}