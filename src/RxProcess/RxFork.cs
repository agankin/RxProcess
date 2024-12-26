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
    /// The empty instance returned on forking an already forked process.
    /// </summary>
    public static RxFork None { get; } = new RxFork();

    /// <summary>
    /// If called from the master process starts the forked process.
    /// </summary>
    /// <remarks>
    /// This method does nothing if called from a forked process.
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
    /// This method does nothing if called from a forked process.
    /// </remarks>
    public void Kill()
    {
        if (_isInMaster)
            _rxProcess.Kill();
    }

    /// <summary>
    /// If called from the master process sends a line to the forked process to the standard input.
    /// </summary>
    /// <remarks>
    /// This method does nothing if called from a forked process.
    /// </remarks>
    public void SendLine(string line)
    {
        if (_isInMaster)
            _rxProcess.SendLine(line);
    }

    /// <summary>
    /// Invokes a delegate if called from the master process.
    /// </summary>
    /// <remarks>
    /// This method does nothing if called from a forked process.
    /// </remarks>
    /// <param name="action">A delegate.</param>
    public void RunInMaster(Action<RxProcess> action)
    {
        if (_isInMaster)
            action(_rxProcess);
    }

    /// <summary>
    /// Invokes a delegate if called from the master process.
    /// </summary>
    /// <remarks>
    /// This method does nothing if called from a forked process.
    /// </remarks>
    /// <param name="action">A delegate.</param>
    public async Task RunInMasterAsync(Func<RxProcess, Task> asyncAction)
    {
        if (_isInMaster)
            await asyncAction(_rxProcess);
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