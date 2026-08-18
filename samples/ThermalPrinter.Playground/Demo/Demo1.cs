using System.Text;
using ThermalPrinter.Core.Enums;
using ThermalPrinter.Core.Transports;
using ThermalPrinter.TSC;

namespace ThermalPrinter.Playground.Demo;

public static class Demo1
{
    public static async Task SendPrintJobAsync()
    {        
        string printerIP = "192.168.0.157";
        int printerPort = 9100;
        bool isBidirectional = true;

        // Initialize printer instance
        using var transport = new NetworkTransport(printerIP, printerPort, isBidirectional);
        var printer = new TscPrinter(transport);

        Console.WriteLine("\n=== Demo 1: Sending printer commands to network printer ===");        

        try
        {
            Console.WriteLine($"[1/3] Connecting to printer natively at {printerIP}:{printerPort}...");            
            await printer.ConnectAsync();
            Console.WriteLine("Connected!");

            
            Console.WriteLine("\n[2/3] Getting printer status...");            
            PrinterStatus status = await printer.GetStatusAsync();
            Console.WriteLine($"Printer Status: {status}");

            
            Console.WriteLine("\n[3/3] Printing raw data...");            

            // RAW TSPL Command Execution in Greek language
            string tsplCommands =
                "SET RIBBON OFF\r\n" +
                "SIZE 105.00 mm,74.00 mm\r\n" +
                "GAP 3 mm,0 mm\r\n" +
                "DIRECTION 1,0\r\n" +
                "CLS\r\n" +
                "SPEED 4\r\n" +
                "DENSITY 9\r\n" +
                "CODEPAGE 1253\r\n" +
                "SET CUTTER OFF\r\n" +
                "SET PEEL OFF\r\n" +
                "REFERENCE 0,0\r\n" +
                "TEXT 40, 30, \"ROMAN.TTF\", 0, 10, 10, 1, \"TEST PRINT DIRECT\"\r\n" +
                "TEXT 800, 30, \"ROMAN.TTF\", 0, 10, 10, 3, \"ΔΟΚΙΜΑΣΤΙΚΗ ΕΚΤΥΠΩΣΗ\"\r\n" +
                "PRINT 1,1\r\n";

            Encoding GreekEncoding = Encoding.GetEncoding(1253); // Greek (Windows) code page
            byte[] rawData = GreekEncoding.GetBytes(tsplCommands);

            await printer.PrintRawAsync(rawData);

            Console.WriteLine("Print job sent successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR]: {ex.Message}");
        }
        finally
        {
            await printer.DisconnectAsync();
        }
    }
}