// samples/ThermalPrinter.Playground/Program.cs
using System.Text;
using ThermalPrinter.Playground.Demo;

// Register the code page provider to support multiple encodings
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// await Demo1.Execute();
// await Demo2.Execute();
// await Demo3.Execute();
await Demo4.Execute();