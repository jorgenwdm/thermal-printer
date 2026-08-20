using System.Text;
using ThermalPrinter.Core.Enums;
using ThermalPrinter.Core.Transports;
using ThermalPrinter.TSC;

namespace ThermalPrinter.Playground.Demo;

/// <summary>
/// Demonstration class for reading stored files from a network thermal printer
/// </summary>
public static class Demo3
{
    public static async Task Execute()
    {
        string printerIP = "192.168.0.157";
        int printerPort = 9100;
        bool isBidirectional = true;

        using var transport = new NetworkTransport(printerIP, printerPort, isBidirectional);
        var printer = new TscPrinter(transport);

        Console.WriteLine("\n=== Demo 3: Retrieve stored files from network thermal printer ===");

        try
        {
            Console.WriteLine($"[1/2] Connecting to network printer at {printerIP}:{printerPort}...");
            await printer.ConnectAsync();
            Console.WriteLine("--> TCP Connection Established with network printer.");

            Console.WriteLine("\n[2/2] Attempting file reading (15 seconds timeout)...");

            // Explicit 15-second execution budget for readback
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            List<string> files = await printer.GetFilesAsync(cts.Token);

            if (files.Count > 0)
            {
                Console.WriteLine($"[RESULT] Found {files.Count} file(s):\n{string.Join("\n", files)}");
            }
            else
            {
                Console.WriteLine("[RESULT] No files found or no response received.");
            }
        }        
        catch (OperationCanceledException)
        {
            Console.WriteLine("[ERROR]: File retrieval timed out after 15 seconds.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR]: {ex.Message}");
        }
        finally
        {
            await printer.DisconnectAsync();            
            Console.WriteLine("\nDisconnected.");
        }
    }
}