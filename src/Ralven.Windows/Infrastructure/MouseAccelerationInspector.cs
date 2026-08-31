using System.Runtime.InteropServices;

namespace Ralven.Windows.Infrastructure;

public enum MouseAccelerationInspectionState
{
    Available,
    Unavailable
}

public sealed record MouseAccelerationSnapshot(
    MouseAccelerationInspectionState State,
    int? Threshold1,
    int? Threshold2,
    int? AccelerationLevel)
{
    public static MouseAccelerationSnapshot Unavailable { get; } = new(
        MouseAccelerationInspectionState.Unavailable,
        null,
        null,
        null);
}

public interface IMouseAccelerationInspector
{
    MouseAccelerationSnapshot GetSnapshot();
}

/// <summary>
/// Reads the three values exposed by SPI_GETMOUSE. It never changes pointer
/// settings and reports API failures explicitly instead of inferring defaults.
/// </summary>
public sealed class WindowsMouseAccelerationInspector : IMouseAccelerationInspector
{
    private const uint SpiGetMouse = 0x0003;

    private readonly Func<int[], bool> readMouse;

    public WindowsMouseAccelerationInspector()
        : this(ReadMouse)
    {
    }

    internal WindowsMouseAccelerationInspector(Func<int[], bool> readMouse)
    {
        this.readMouse = readMouse ?? throw new ArgumentNullException(nameof(readMouse));
    }

    public MouseAccelerationSnapshot GetSnapshot()
    {
        var values = new int[3];
        try
        {
            return readMouse(values)
                ? new MouseAccelerationSnapshot(
                    MouseAccelerationInspectionState.Available,
                    values[0],
                    values[1],
                    values[2])
                : MouseAccelerationSnapshot.Unavailable;
        }
        catch (Exception exception) when (exception is not (
            OutOfMemoryException or StackOverflowException or AccessViolationException))
        {
            return MouseAccelerationSnapshot.Unavailable;
        }
    }

    private static bool ReadMouse(int[] values) =>
        SystemParametersInfo(SpiGetMouse, 0, values, 0);

    [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(
        uint action,
        uint parameter,
        [Out] int[] values,
        uint flags);
}
