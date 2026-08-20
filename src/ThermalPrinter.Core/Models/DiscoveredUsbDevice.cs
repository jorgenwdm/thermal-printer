namespace ThermalPrinter.Core.Models;

/// <summary>
/// Represents a discovered USB device during system scanning.
/// </summary>
public record DiscoveredUsbDevice(
    int VendorId,
    int ProductId,
    string Manufacturer,
    string ProductName,
    string SerialNumber,
    string DeviceClass,
    bool HasPermission
)
{
    public string VidHex => $"0x{VendorId:X4}";
    public string PidHex => $"0x{ProductId:X4}";

    public override string ToString()
    {
        return $"[{VidHex}:{PidHex}] - Device: {ProductName} ({Manufacturer}) - Device Class: {DeviceClass} | Permissions: {(HasPermission ? "Granted" : "Denied")}";
    }
}