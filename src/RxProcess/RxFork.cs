namespace RxProcessLib;

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
    /// </summary>
    /// <remarks>
    /// The method does nothing if called from a forked process.
    /// </remarks>
    public void Start()
    {
        if (_isInMaster)
            _rxProcess.Start();
    }

    /// <summary>
    /// If called from the master process kills the forked process.
    /// </summary>
    /// <remarks>
    /// The method does nothing if called from a forked process.
    /// </remarks>
    public void Kill()
    {
        if (_isInMaster)
            _rxProcess.Kill();
    }

    /// <summary>
    /// Invokes a delegate if called from the master process.
    /// </summary>
    /// <remarks>
    /// The method does nothing if called from a forked process.
    /// </remarks>
    /// <param name="action">A delegate.</param>
    public void RunInMaster(Action<RxProcess> action)
    {
        if (_isInMaster)
            action?.Invoke(_rxProcess);
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