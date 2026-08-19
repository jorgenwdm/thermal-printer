// samples/ThermalPrinter.Playground/Program.cs
using System.Text;
using ThermalPrinter.Playground.Demo;

// Register the code page provider to support multiple encodings
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

await Demo1.SendPrintJobAsync();
// await Demo2.TestPrintServerStatusAsync();
// await Demo3.TestPrinterGetFilesAsync();