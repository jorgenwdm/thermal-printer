// src/ThermalPrinter.Core/Interfaces/IThermalPrinter.cs
using ThermalPrinter.Core.Enums;

namespace ThermalPrinter.Core.Interfaces;

public interface IThermalPrinter
{
    /// <summary>
    /// Establishes connection to a thermal printer
    /// </summary>    
    Task ConnectAsync(CancellationToken cancellationToken = default);
    

    /// <summary>
    /// Closes connection to a thermal printer
    /// </summary>
    /// <returns></returns>
    Task DisconnectAsync();


    /// <summary>
    /// Retrieve printer status
    /// </summary>    
    Task<PrinterStatus> GetStatusAsync(CancellationToken cancellationToken = default);


    /// <summary>
    /// Retrieves a list of files on the printer
    /// </summary>        
    Task<List<string>> GetFilesAsync(CancellationToken cancellationToken = default);


    /// <summary>
    /// Sends a print job command (raw byte data) to the thermal printer
    /// </summary>
    /// <param name="rawData"></param>        
    Task PrintAsync(byte[] rawData, CancellationToken cancellationToken = default);


    /// <summary>
    /// Downloads a font file to the printer memory.
    /// </summary>
    /// <param name="fontName">Target filename on printer (e.g. "ROMAN.TTF").</param>
    /// <param name="fontData">Font binary bytes.</param>
    /// <param name="target">Target memory location (Default: Flash memory).</param>
    Task DownloadFontAsync( string fontName, byte[] fontData, MemoryTarget target = MemoryTarget.Flash, CancellationToken cancellationToken = default);


    /// <summary>
    /// Downloads a bitmap file to the printer memory.
    /// </summary>
    /// <param name="imageName">Target filename on printer (e.g. "LOGO.BMP").</param>
    /// <param name="bitmapData">Bitmap binary bytes.</param>
    /// <param name="target">Target memory location (Default: Flash memory).</param>
    public Task DownloadBitmapAsync( string imageName, byte[] bitmapData, MemoryTarget target = MemoryTarget.Flash, CancellationToken cancellationToken = default);
        
}




    