using System.Diagnostics;

namespace RxProcess;

/// <summary>
/// Represensts a process as an observable reactively providing stdout/stderr data.
/// </summary>
public class RxProcess : IObservable<StdOutLine>, IDisposable
{
    private readonly RxSubscriptionSet _subscriptions = new();
    private readonly Process _process;

    private volatile int _state = (int)RxProcessState.Unstarted;

    private RxProcess(ProcessStartInfo startInfo)
    {
        _process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };;

        SubscribeAllEvents();
    }

    /// <summary>
    /// The process state.
    /// </summary>
    public RxProcessState State => (RxProcessState)_state;

    /// <summary>
    /// Creates new instance of <see cref="RxProcess"/>.
    /// </summary>
    /// <param name="programFile">A file name to start.</param>
    /// <param name="args">Command line arguments that will be passed to the started process.</param>
    /// <returns>A new instance of <see cref="RxProcess"/>.</returns>
    public static RxProcess Create(string programFile, params string[] args) => Create(programFile, args.AsEnumerable());
    
    /// <summary>
    /// Starts a process.
    /// </summary>
    /// <param name="programFile">A file name to start.</param>
    /// <param name="args">Command line arguments that will be passed to the started process.</param>
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
    public IDisposable Subscribe(IObserver<StdOutLine> observer) => _subscriptions.Add(observer);

    /// <inheritdoc/>
    public void Dispose()
    {
        UnsubscribeAllEvents();
        _process.Dispose();
    }

    private void OnOutLineReceived(object sender, DataReceivedEventArgs e)
    {
        var line = StdOutLine.Out(e.Data ?? string.Empty);
        foreach (var subscription in _subscriptions)
            subscription.OnNext(line);
    }

    private void OnErrLineReceived(object sender, DataReceivedEventArgs e)
    {
        var line = StdOutLine.Err(e.Data ?? string.Empty);
        foreach (var subscription in _subscriptions)
            subscription.OnNext(line);
    }

    private void OnExited(object? sender, EventArgs e)
    {
        _state = (int)RxProcessState.Exited;

        UnsubscribeAllEvents();
        foreach (var subscription in _subscriptions)
            subscription.Complete();
    }

    private void SubscribeAllEvents()
    {
        _process.OutputDataReceived += OnOutLineReceived;
        _process.ErrorDataReceived += OnErrLineReceived;
        _process.Exited += OnExited;
    }
    
    private void UnsubscribeAllEvents()
    {
        _process.OutputDataReceived -= OnOutLineReceived;
        _process.ErrorDataReceived -= OnErrLineReceived;
        _process.Exited -= OnExited;
    }

    private static void ThrowWrongState(RxProcessState state) =>
        throw new InvalidOperationException($"The process is in {state} state.");
}