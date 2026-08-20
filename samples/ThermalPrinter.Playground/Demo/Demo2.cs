using System.Text;
using ThermalPrinter.Core.Enums;
using ThermalPrinter.Core.Exceptions;
using ThermalPrinter.Core.Transports;
using ThermalPrinter.TSC;

namespace ThermalPrinter.Playground.Demo;

/// <summary>
/// Demonstration class for reading printer status from an external print server attached to a thermal printer
/// Network connection to a print server is not biderectional so a status reading from the print server is not possible and an exception must be thrown
/// </summary>
public static class Demo2
{
    public static async Task Execute()
    {
        string printServerIP = "192.168.0.49";
        int printerPort = 9100;
        
        // this print server does not support bidirectional communication)
        // We set false because we can not read something back from the printer.
        // If you set true, an OperationCanceledException is thrown after 15 seconds since the print server can not send anything back
        bool isBidirectional = false;          

        // Initialize communication transport
        using var transport = new NetworkTransport(printServerIP, printerPort, isBidirectional);
        
        // Intialize printer instance
        var printer = new TscPrinter(transport);


        Console.WriteLine("\n=== Demo 2: External Print Server Status ===\n");


        try
        {
            Console.WriteLine($"[1/2] Connecting to Print Server at {printServerIP}:{printerPort}...");
            await printer.ConnectAsync();   // connect to print server            
            Console.WriteLine("--> TCP Connection Established with Print Server.");
            

            Console.WriteLine("\n[2/2] Attempting Status Readback via External Print Server...");
            PrinterStatus status = await printer.GetStatusAsync();  // read printer status (an exception must be thrown since the print server is not bidirectional)
            Console.WriteLine($"[RESULT] Status Received: {status}");

        }
        catch (TransportUnidirectionalException ex)
        {
            // When isBidirectional = false a TransportUnidirectionalException is thrown
            Console.WriteLine($"[EXPTECTED ERROR]: {ex.Message}");  
        }
        catch (OperationCanceledException ex)
        {
            // if you set isBidirectional = true, an OperationCanceledException is thrown after 15 seconds
            // because the print server does not really support bidirectional communication
            Console.WriteLine($"[TIMEOUT ERROR]: {ex.Message}");   
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR]: {ex.Message}");
        }
        finally
        {
            await printer.DisconnectAsync();    // disconnect
            Console.WriteLine("\nDisconnected.");
        }
    }
}