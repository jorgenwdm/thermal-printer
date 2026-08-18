// src/ThermalPrinter.Core/Enums/PrinterStatus.cs
namespace ThermalPrinter.Core.Enums;

[Flags]
public enum PrinterStatus
{
    Ready = 0,
    PaperOut = 1 << 0,
    RibbonEnd = 1 << 1,
    CoverOpen = 1 << 2,
    Pause = 1 << 3,
    Error = 1 << 4,
    Offline = 1 << 5
}