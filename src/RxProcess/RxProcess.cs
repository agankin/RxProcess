using System.Diagnostics;

namespace RxProcess;

/// <summary>
/// Represensts a process as an observable reactively providing stdout/stderr data.
/// </summary>
public class RxProcess : IObservable<StdOutLine>, IDisposable
{
    private readonly ConcurrentSet<IObserver<StdOutLine>> _observers = [];
    private readonly Process _process;

    private RxProcess(Process process)
    {
        _process = process;

        _process.OutputDataReceived += OnOutLineReceived;
        _process.ErrorDataReceived += OnErrLineReceived;
        _process.Exited += OnExited;
    }

    /// <summary>
    /// Starts a process.
    /// </summary>
    /// <param name="programFile">File name to start.</param>
    /// <param name="args">Command line arguments to pass to the started process.</param>
    /// <returns>An instance of <see cref="RxProcess"/>.</returns>
    public static RxProcess Start(string programFile, params string[] args) => Start(programFile, args.AsEnumerable());
    
    /// <summary>
    /// Starts a process.
    /// </summary>
    /// <param name="programFile">File name to start.</param>
    /// <param name="args">Command line arguments to pass to the started process.</param>
    /// <returns>An instance of <see cref="RxProcess"/>.</returns>
    public static RxProcess Start(string programFile, IEnumerable<string> args)
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
        
        var process = new Process
        {
            StartInfo = startInfo
        };

        var rxProcess = new RxProcess(process);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return rxProcess;
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
}