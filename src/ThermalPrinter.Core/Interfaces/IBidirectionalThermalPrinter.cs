namespace ThermalPrinter.Core.Interfaces;

/// <summary>
/// Interface for bidirectional transactions (command and query requests) to a thermal printer
/// </summary>
public interface IBidirectionalThermalPrinter : ICommandThermalPrinter, IQueryThermalPrinter
{
   
}