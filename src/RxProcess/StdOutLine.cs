namespace RxProcess;

/// <summary>
/// Standard output text line.
/// </summary>
/// <param name="Value">Text line.</param>
/// <param name="Type">Standard output type.</param>
public readonly record struct StdOutLine(
    string Value,
    StdOutType Type
)
{
    /// <summary>
    /// Checks if a line instance is from the standard output.
    /// </summary>
    /// <param name="line">A line.</param>
    public static bool IsOut(StdOutLine line) => line.Type == StdOutType.Err;
    
    /// <summary>
    /// Checks if a line instance is from the standard error.
    /// </summary>
    /// <param name="line">A line.</param>
    public static bool IsErr(StdOutLine line) => line.Type == StdOutType.Err;

    internal static StdOutLine Out(string line) => new StdOutLine(line, StdOutType.Out);

    internal static StdOutLine Err(string line) => new StdOutLine(line, StdOutType.Err);
}