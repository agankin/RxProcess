using System.Diagnostics;

namespace RxProcess;

/// <summary>
/// Represensts a process as an observable reactively providing stdout/stderr data.
/// </summary>
public class RxProcess : IObservable<StdOutLine>, IDisposable
{
    private readonly ConcurrentSet<IObserver<StdOutLine>> _observers = [];
    private readonly Process _process;

    private volatile int _state = (int)RxProcessState.Unstarted;

    private RxProcess(ProcessStartInfo startInfo)
    {
        _process = new Process
        {
            StartInfo = startInfo
        };;

        _process.OutputDataReceived += OnOutLineReceived;
        _process.ErrorDataReceived += OnErrLineReceived;
        _process.Exited += OnExited;
    }

    /// <summary>
    /// The process state.
    /// </summary>
    public RxProcessState State => (RxProcessState)_state;

    /// <summary>
    /// Creates new instance  process.
    /// </summary>
    /// <param name="programFile">File name to start.</param>
    /// <param name="args">Command line arguments to pass to the started process.</param>
    /// <returns>An instance of <see cref="RxProcess"/>.</returns>
    public static RxProcess Create(string programFile, params string[] args) => Create(programFile, args.AsEnumerable());
    
    /// <summary>
    /// Starts a process.
    /// </summary>
    /// <param name="programFile">File name to start.</param>
    /// <param name="args">Command line arguments to pass to the started process.</param>
    /// <returns>An instance of <see cref="RxProcess"/>.</returns>
    public static RxProcess Create(string programFile, IEnumerable<string> args)
    {
        var argsLine = string.Join(" ", args);

        var startInfo = new ProcessStartInfo()
        {
            FileName = programFile,
            Arguments = argsLine,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        return new RxProcess(startInfo);
    }

    /// <summary>
    /// Starts the process.
    /// </summary>
    public void Start()
    {
        var state = (RxProcessState)Interlocked.CompareExchange(ref _state, (int)RxProcessState.Running, (int)RxProcessState.Unstarted);
        if (state != RxProcessState.Unstarted)
            ThrowWrongState(state);
        
        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
    }

    /// <summary>
    /// Kills the process.
    /// </summary>
    public void Kill()
    {
        if (_state != (int)RxProcessState.Running)
            ThrowWrongState((RxProcessState)_state);

        _process.Kill();
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<StdOutLine> observer)
    {
        _observers.Add(observer);
        return new DoDisposable(() => _observers.Remove(observer));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        UnsubscribeAll();
        _process.Dispose();
    }

    private void OnOutLineReceived(object sender, DataReceivedEventArgs e)
    {
        var line = StdOutLine.Out(e.Data ?? string.Empty);
        foreach (var observer in _observers)
            observer.OnNext(line);
    }

    private void OnErrLineReceived(object sender, DataReceivedEventArgs e)
    {
        var line = StdOutLine.Err(e.Data ?? string.Empty);
        foreach (var observer in _observers)
            observer.OnNext(line);
    }

    private void OnExited(object? sender, EventArgs e)
    {
        _state = (int)RxProcessState.Exited;

        UnsubscribeAll();
        foreach (var observer in _observers)
            observer.OnCompleted();
    }

    private void UnsubscribeAll()
    {
        _process.OutputDataReceived -= OnOutLineReceived;
        _process.ErrorDataReceived -= OnErrLineReceived;
        _process.Exited -= OnExited;
    }

    private static void ThrowWrongState(RxProcessState state)
    {
        throw new InvalidOperationException($"The process is in {state} state.");
    }
}