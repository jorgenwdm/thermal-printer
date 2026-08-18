// src/ThermalPrinter.Core/Interfaces/IThermalPrinter.cs
using ThermalPrinter.Core.Enums;

namespace ThermalPrinter.Core.Interfaces;

public interface IThermalPrinter
{
    /// <summary>
    /// Establishes connection to a thermal printer
    /// </summary>    
    Task ConnectAsync(CancellationToken cancellationToken = default);
    
    Task DisconnectAsync();

    /// <summary>
    /// Retrieve printer status
    /// </summary>
    /// <param name="cancellationToken"></param>    
    Task<PrinterStatus> GetStatusAsync(CancellationToken cancellationToken = default);
    
    Task PrintRawAsync(byte[] rawData, CancellationToken cancellationToken = default);
    
    Task DownloadFontAsync(string fontName, byte[] fontData, CancellationToken cancellationToken = default);
    
    Task DownloadBitmapAsync(string imageName, byte[] bitmapData, CancellationToken cancellationToken = default);

    Task<List<string>> GetFilesAsync(CancellationToken cancellationToken = default);
}