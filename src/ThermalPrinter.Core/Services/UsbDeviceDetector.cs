using System;
using System.Collections.Generic;
using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using ThermalPrinter.Core.Models;

namespace ThermalPrinter.Core.Services;

public class UsbDeviceDetector : IDisposable
{
    private const string AccessDeniedText = "Access Denied";
    private readonly UsbContext _context;

    public UsbDeviceDetector()
    {
        _context = new UsbContext();
    }

    /// <summary>
    /// Scans all connected USB devices and retrieves their metadata and Device/Interface Class.
    /// </summary>
    public List<DiscoveredUsbDevice> ScanAllDevices()
    {
        var result = new List<DiscoveredUsbDevice>();
        var allDevices = _context.List();

        foreach (var device in allDevices)
        {
             // 1. First we read the device class
            string deviceClass = "Unknown";
            if (device.Configs.Count > 0 && device.Configs[0].Interfaces.Count > 0)
            {
                ClassCode classCode = device.Configs[0].Interfaces[0].Class;
                deviceClass = classCode.ToString();
            }

            string manufacturer;
            string productName;
            string serialNumber;
            bool hasPermission = false;

            try
            {
                // We try to open the device and retrieve its metadata
                device.Open();
                hasPermission = true;

                // 2. We read the string descriptors
                manufacturer = !string.IsNullOrWhiteSpace(device.Info.Manufacturer) 
                    ? device.Info.Manufacturer 
                    : "N/A";

                productName = !string.IsNullOrWhiteSpace(device.Info.Product) 
                    ? device.Info.Product 
                    : "N/A";

                serialNumber = !string.IsNullOrWhiteSpace(device.Info.SerialNumber) 
                    ? device.Info.SerialNumber 
                    : "N/A";

                device.Close();
            }
            catch
            {
                // In case no permissions are available
                manufacturer = AccessDeniedText;
                productName = AccessDeniedText;
                serialNumber = AccessDeniedText;                
                hasPermission = false;
            }

            result.Add(new DiscoveredUsbDevice(
                VendorId: device.VendorId,
                ProductId: device.ProductId,
                Manufacturer: manufacturer,
                ProductName: productName,
                SerialNumber: serialNumber,
                DeviceClass: deviceClass,
                HasPermission: hasPermission
            ));
        }

        return result;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}