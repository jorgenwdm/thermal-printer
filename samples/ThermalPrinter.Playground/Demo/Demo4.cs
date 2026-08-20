using System.Text;
using ThermalPrinter.Core.Enums;
using ThermalPrinter.Core.Models;
using ThermalPrinter.Core.Services;
using ThermalPrinter.Core.Transports;
using ThermalPrinter.TSC;

namespace ThermalPrinter.Playground.Demo;

/// <summary>
/// Demonstration class for detecting usb devices
/// </summary>
public static class Demo4
{
    public static async Task Execute()
    {
        

        Console.WriteLine("\n=== Demo 4: Detect usb devices and additionally list all connected usb printers ===");

        using var detector = new UsbDeviceDetector();

        // 1. Find all connected printers (Auto-Detect)
        var detectedPrinters = detector.ScanAllDevices();

        foreach (var (index, element) in detectedPrinters.Index())
        {
            //Console.WriteLine($"Found Usb Device: {element} | Serial: {element.SerialNumber} | Manufacturer: {element.Manufacturer} | Product: {element.ProductName} | Class: {element.DeviceClass} | Access: {(element.HasPermission ? "OK" : "Denied")}");
            Console.WriteLine($"Usb Device #{index+1}: {element}");
        }
        
        // 2. Check if a specific printer is connected (eg. TSC - VID: 0x1103, PID: 0x0001)
        //var tscPrinter = detector.DetectPrinters(targetVid: 0x1103, targetPid: 0x0001).FirstOrDefault();

        //if (tscPrinter != null)
        //{
        //    Console.WriteLine($"TSC Printer Connected: {tscPrinter.VidHex}:{tscPrinter.PidHex}");
        //}

    }

}