using System.Text;
using ThermalPrinter.Core.Enums;
using ThermalPrinter.Core.Transports;
using ThermalPrinter.TSC;

namespace ThermalPrinter.Playground.Demo;

public static class Demo3
{
    public static async Task TestPrinterGetFilesAsync()
    {
        string printerIP = "192.168.0.157";
        int printerPort = 9100;
        bool isBidirectional = true;

        // Initialize printer instance
        using var transport = new NetworkTransport(printerIP, printerPort, isBidirectional);
        var printer = new TscPrinter(transport);

        Console.WriteLine("\n=== Demo 3: Retrieve stored files from network printer ===");

        try
        {
            Console.WriteLine($"[1/2] Connecting to network printer at {printerIP}:{printerPort}...");
            await printer.ConnectAsync();
            Console.WriteLine("--> TCP Connection Established with network printer.");

            Console.WriteLine("\n[2/2] Attempting file reading...");

            // We define a cancellation token to avoid hanging the ReadAsync if the print server swallows the command
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

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
            Console.WriteLine("[ERROR]: File retrieval timed out.");
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