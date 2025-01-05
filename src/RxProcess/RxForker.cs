using System.Reflection;

namespace RxProcessLib;

/// <summary>
/// Forks creation and control utility.
/// </summary>
public static class RxForker
{
    private const string DotnetCmd = "dotnet";
    private const string ForkEnvironmentVariable = "RxForker.Fork";

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

        var rxProcess = RxProcess.Create(DotnetCmd, args.Prepend(entry))
            .SetEnvironmentVariable(ForkEnvironmentVariable, "true");

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
            action();
    }

    /// <summary>
    /// Invokes a delegate if called from the master process.
    /// </summary>
    /// <remarks>
    /// This method does nothing if called from a fork process.
    /// </remarks>
    /// <param name="action">A delegate.</param>
    public static async Task RunInMasterAsync(Func<Task> asyncAction)
    {
        if (!IsForked())
            await asyncAction();
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
            action();
    }

    /// <summary>
    /// Invokes a delegate if called from a fork process.
    /// </summary>
    /// <remarks>
    /// This method does nothing if called from the master process.
    /// </remarks>
    /// <param name="action">A delegate.</param>
    public static async Task RunInForkAsync(Func<Task> asyncAction)
    {
        if (IsForked())
            await asyncAction();
    }

    private static bool IsForked() => Environment.GetEnvironmentVariable(ForkEnvironmentVariable) is not null;
}