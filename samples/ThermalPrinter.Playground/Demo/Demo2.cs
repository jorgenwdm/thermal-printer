using System.Text;
using ThermalPrinter.Core.Enums;
using ThermalPrinter.Core.Transports;
using ThermalPrinter.TSC;

namespace ThermalPrinter.Playground.Demo;

public static class Demo2
{
    public static async Task TestPrintServerStatusAsync()
    {
        string printServerIP = "192.168.0.49";
        int printerPort = 9100;
        bool isBidirectional = false;  // false because most print servers do not support bidirectional communication

        // Initialize printer instance
        using var transport = new NetworkTransport(printServerIP, printerPort, isBidirectional);
        var printer = new TscPrinter(transport);

        Console.WriteLine("\n=== Demo 2: External USB Print Server Status ===");

        try
        {
            Console.WriteLine($"[1/2] Connecting to Print Server at {printServerIP}:{printerPort}...");
            await printer.ConnectAsync();
            Console.WriteLine("--> TCP Connection Established with Print Server.");

            
            Console.WriteLine("\n[2/2] Attempting Status Readback via External Print Server...");

            // We define a short Cancellation Token to avoid hanging the ReadAsync if the print server swallows the status command and never responds.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

            PrinterStatus status = await printer.GetStatusAsync(cts.Token);

            Console.WriteLine($"[RESULT] Status Received: {status}");
        }
        catch (NotSupportedException)
        {
            Console.WriteLine("[EXPECTED BEHAVIOR] Read Timeout: The External Print Server does not support status response back over Network.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR]: {ex.Message}");
        }
        finally
        {
            await printer.DisconnectAsync();            
            Console.WriteLine("Disconnected.");
        }
    }
}