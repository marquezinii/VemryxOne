using System.Management;
using System.Runtime.InteropServices;

namespace Ralven.Windows.Infrastructure;

public sealed record CpuSnapshot(
    int PhysicalCores,
    int LogicalThreads,
    uint CurrentClockMhz,
    uint MaxClockMhz);

public interface ICpuInspector
{
    CpuSnapshot? GetSnapshot();
}

/// <summary>
/// Reads CPU inventory from WMI Win32_Processor — a standard, driver-free
/// class already present on every supported Windows edition. Returns null
/// when the read fails; callers must report that honestly instead of
/// guessing. Follows the same try/graceful-null WMI pattern already used for
/// RAM module layout in <c>AppOptimizationService</c>.
/// </summary>
public sealed class WindowsCpuInspector : ICpuInspector
{
    private static readonly TimedSnapshotCache<CpuSnapshot> Cache = new();

    public CpuSnapshot? GetSnapshot() => Cache.GetOrReadOptional(
        () => ReadSafely(ReadProcessors));

    internal static CpuSnapshot? ReadSafely(Func<IReadOnlyList<CpuSnapshot>> read)
    {
        try
        {
            return Aggregate(read());
        }
        catch (Exception exception) when (exception is ManagementException
            or COMException
            or UnauthorizedAccessException)
        {
            return null;
        }
    }

    internal static CpuSnapshot? Aggregate(IEnumerable<CpuSnapshot> processors)
    {
        var valid = processors.Where(processor => processor is
        {
            PhysicalCores: > 0,
            LogicalThreads: > 0,
            CurrentClockMhz: > 0,
            MaxClockMhz: > 0
        }).ToArray();
        if (valid.Length == 0)
        {
            return null;
        }

        var physicalCores = valid.Sum(processor => (long)processor.PhysicalCores);
        var logicalThreads = valid.Sum(processor => (long)processor.LogicalThreads);
        if (physicalCores > int.MaxValue || logicalThreads > int.MaxValue)
        {
            return null;
        }

        var currentClock = valid.Sum(processor =>
            (double)processor.CurrentClockMhz * processor.PhysicalCores) / physicalCores;
        var maxClock = valid.Sum(processor =>
            (double)processor.MaxClockMhz * processor.PhysicalCores) / physicalCores;
        return new CpuSnapshot(
            (int)physicalCores,
            (int)logicalThreads,
            checked((uint)Math.Round(currentClock)),
            checked((uint)Math.Round(maxClock)));
    }

    private static IReadOnlyList<CpuSnapshot> ReadProcessors()
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT NumberOfCores, NumberOfLogicalProcessors, CurrentClockSpeed, MaxClockSpeed "
                + "FROM Win32_Processor");
        using var results = searcher.Get();
        var processors = new List<CpuSnapshot>();
        foreach (ManagementObject processor in results.Cast<ManagementObject>())
        {
            using (processor)
            {
                var cores = processor["NumberOfCores"] as uint?;
                var threads = processor["NumberOfLogicalProcessors"] as uint?;
                var current = processor["CurrentClockSpeed"] as uint?;
                var max = processor["MaxClockSpeed"] as uint?;
                if (cores is > 0 and <= int.MaxValue
                    && threads is > 0 and <= int.MaxValue
                    && current is > 0
                    && max is > 0)
                {
                    processors.Add(new CpuSnapshot(
                        (int)cores.Value,
                        (int)threads.Value,
                        current.Value,
                        max.Value));
                }
            }
        }

        return processors;
    }
}
