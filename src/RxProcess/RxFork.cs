using System.Reflection;

namespace RxProcess;

public static class RxFork
{
    private const string DotnetCmd = "dotnet";
    private const string ForkedArg = "--forked";

    public static void Fork(Action<RxProcess>? onForked = null)
    {
        if (IsForked())
            return;

        var entry = Assembly.GetExecutingAssembly().Location;
        var rxProcess = RxProcess.Start(DotnetCmd, entry, ForkedArg);

        onForked?.Invoke(rxProcess);
    }

    private static bool IsForked() => Environment.GetCommandLineArgs()
        .Any(args => string.Equals(args, ForkedArg, StringComparison.OrdinalIgnoreCase));
}