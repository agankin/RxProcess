namespace RxProcessLib;

/// <summary>
/// Contains possible process states.
/// </summary>
public enum RxProcessState
{
    /// <summary>
    /// Not started yet.
    /// </summary>
    Unstarted,

    /// <summary>
    /// Running.
    /// </summary>
    Running,

    /// <summary>
    /// Exited.
    /// </summary>
    Exited,

    /// <summary>
    /// Disposed.
    /// </summary>
    Disposed
}