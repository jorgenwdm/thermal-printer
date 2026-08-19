// src/ThermalPrinter.Core/Enums/TscMemoryTarget.cs
namespace ThermalPrinter.Core.Enums;

public enum MemoryTarget
{
    Dram,       // Temporary memory module
    Flash,      // Main board flash memory (Persistent)
    Expansion   // Expansion memory module
}