namespace RxProcess;

/// <summary>
/// Represents a fork of the current process.
/// </summary>
public class RxFork
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
    public static RxFork EmptyForAlreadyForked { get; } = new RxFork();

    /// <summary>
    /// If called from the master process starts the forked process.
    /// If called from a forked process does nothing.
    /// </summary>
    public void StartInMaster()
    {
        if (_isInMaster)
            _rxProcess.Start();
    }

    /// <summary>
    /// If called from the master process invokes the provided delegate with passing an instance of <see cref="RxProcess"/>
    /// representing the forked process.
    /// If called from a forked process does nothing.
    /// </summary>
    /// <param name="action">A delegate.</param>
    public void HandleInMaster(Action<RxProcess> action)
    {
        if (_isInMaster)
            action?.Invoke(_rxProcess);
    }
}