namespace RxProcess;

public readonly record struct StdOutLine(
    string Value,
    StdOutType Type
)
{
    public static StdOutLine Out(string line) => new StdOutLine(line, StdOutType.Out);

    public static StdOutLine Err(string line) => new StdOutLine(line, StdOutType.Err);

    public static bool IsStdErrLine(StdOutLine line) => line.Type == StdOutType.Err;
}