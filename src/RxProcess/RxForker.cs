using System.Reflection;

namespace RxProcessLib;

/// <summary>
/// Forks creation and control utility.
/// </summary>
public static class RxForker
{
    private const string DotnetCmd = "dotnet";
    private const string ForkedArg = "--forked";

    /// <summary>
    /// Forks the current master process.
    /// </summary>
    /// <remarks>
    /// If invoked from a fork process returns <see cref="RxFork.None"/> instance without creating a new fork.
    /// </remarks> 
    public static RxFork Fork()
    {
        if (IsForked())
            return RxFork.None;

        var entry = Assembly.GetEntryAssembly().Location;
        var args = Environment.GetCommandLineArgs().AsEnumerable();
        var rxProcess = RxProcess.Create(DotnetCmd, args.Prepend(entry).Append(ForkedArg));

        return new RxFork(rxProcess);
    }

    /// <summary>
    /// Invokes a delegate if called from the master process.
    /// </summary>
    /// <remarks>
    /// This method does nothing if called from a fork process.
    /// </remarks>
    /// <param name="action">A delegate.</param>
    public static void RunInMaster(Action action)
    {
        if (!IsForked())
            action?.Invoke();
    }

    /// <summary>
    /// Invokes a delegate if called from a fork process.
    /// </summary>
    /// <remarks>
    /// This method does nothing if called from the master process.
    /// </remarks>
    /// <param name="action">A delegate.</param>
    public static void RunInFork(Action action)
    {
        if (IsForked())
            action?.Invoke();
    }

    private static bool IsForked() => Environment.GetCommandLineArgs()
        .Any(args => string.Equals(args, ForkedArg, StringComparison.OrdinalIgnoreCase));
}