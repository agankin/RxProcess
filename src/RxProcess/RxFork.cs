namespace RxProcess;

/// <summary>
/// Represents a fork of the current process.
/// </summary>
public class RxFork : IObservable<StdOutLine>, IDisposable
{
    private readonly RxProcess _rxProcess = null!;
    private readonly bool _isInMaster;

    internal RxFork() {}
    
    internal RxFork(RxProcess rxProcess)
    {
        _rxProcess = rxProcess;
        _isInMaster = true;
    }

    /// <summary>
    /// The empty instance returned on an attempt of forking already forked process.
    /// </summary>
    public static RxFork NoneForAlreadyForked { get; } = new RxFork();

    /// <summary>
    /// If called from the master process starts the forked process.
    /// If called from a forked process does nothing.
    /// </summary>
    public void StartInMaster()
    {
        if (_isInMaster)
            _rxProcess.Start();
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<StdOutLine> observer)
    {
        return _isInMaster ? _rxProcess.Subscribe(observer) : new DummyDisposable();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_isInMaster)
            _rxProcess.Dispose();
    }
}