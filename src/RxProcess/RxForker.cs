using System.Reflection;

namespace RxProcess;

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
    /// If invoked from a fork process returns <see cref="RxFork.EmptyForAlreadyForked"/> without creating new fork.
    /// </remarks> 
    public static RxFork Fork()
    {
        if (IsForked())
            return RxFork.EmptyForAlreadyForked;

        var entry = Assembly.GetExecutingAssembly().Location;
        var args = Environment.GetCommandLineArgs().AsEnumerable();
        var rxProcess = RxProcess.Start(DotnetCmd, args.Prepend(entry).Append(ForkedArg));

        return new RxFork(rxProcess);
    }

    /// <summary>
    /// Invokes a delegate if called from a master process.
    /// </summary>
    /// <remarks>
    /// The method does nothing if called from a fork process.
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
    /// The method does nothing if called from a master process.
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