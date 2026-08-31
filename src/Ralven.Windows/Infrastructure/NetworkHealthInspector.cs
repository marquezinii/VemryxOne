using System.Net.NetworkInformation;

namespace Ralven.Windows.Infrastructure;

public sealed record NetworkHealthSnapshot(
    bool HasActiveInterface,
    long DiscardedPackets,
    long ErrorPackets,
    NetworkInterfaceType? ActiveInterfaceType = null,
    long? LinkSpeedBitsPerSecond = null);

public interface INetworkHealthInspector
{
    NetworkHealthSnapshot GetSnapshot();
}

/// <summary>
/// Reads local network interface counters, type and advertised link speed
/// exposed by the OS for active, non-loopback adapters. It never pings an
/// external host and never sends network traffic itself; it only reads
/// statistics the OS already tracks for the adapter.
/// </summary>
public sealed class WindowsNetworkHealthInspector : INetworkHealthInspector
{
    public NetworkHealthSnapshot GetSnapshot()
    {
        NetworkInterface[] interfaces;
        try
        {
            interfaces = NetworkInterface.GetAllNetworkInterfaces();
        }
        catch (NetworkInformationException)
        {
            return new NetworkHealthSnapshot(false, 0, 0);
        }

        var active = interfaces.Where(nic =>
            nic.OperationalStatus == OperationalStatus.Up
            && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback
            && nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

        long discarded = 0;
        long errors = 0;
        var found = false;
        NetworkInterfaceType? fastestInterfaceType = null;
        long? fastestLinkSpeed = null;
        foreach (var nic in active)
        {
            try
            {
                var statistics = nic.GetIPStatistics();
                discarded += statistics.IncomingPacketsDiscarded + statistics.OutgoingPacketsDiscarded;
                errors += statistics.IncomingPacketsWithErrors + statistics.OutgoingPacketsWithErrors;
                found = true;

                var speed = nic.Speed;
                if (speed > 0 && (fastestLinkSpeed is null || speed > fastestLinkSpeed))
                {
                    fastestInterfaceType = nic.NetworkInterfaceType;
                    fastestLinkSpeed = speed;
                }
            }
            catch (NetworkInformationException)
            {
                // A single unreadable adapter should not fail the whole snapshot.
            }
        }

        return new NetworkHealthSnapshot(found, discarded, errors, fastestInterfaceType, fastestLinkSpeed);
    }
}
