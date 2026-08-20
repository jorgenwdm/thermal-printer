using ThermalPrinter.Core.Enums;

namespace ThermalPrinter.Core.Interfaces;

/// <summary>
/// Interface for querying a thermal printer
/// </summary>
public interface IQueryThermalPrinter
{   
    /// <summary>
    /// Retrieve thermal printer status
    /// </summary>    
    Task<PrinterStatus> GetStatusAsync(CancellationToken ct = default);


    /// <summary>
    /// Retrieves a list of files on the thermal printer
    /// </summary>        
    Task<List<string>> GetFilesAsync(CancellationToken ct = default);
}