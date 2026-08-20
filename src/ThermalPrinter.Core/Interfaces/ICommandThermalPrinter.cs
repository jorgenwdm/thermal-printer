using ThermalPrinter.Core.Enums;

namespace ThermalPrinter.Core.Interfaces;

/// <summary>
/// Interface for sending commands to a thermal printer
/// </summary>
public interface ICommandThermalPrinter
{
    /// <summary>
    /// Establishes connection to a thermal printer
    /// </summary>    
    Task ConnectAsync(CancellationToken ct = default);
    

    /// <summary>
    /// Closes connection to a thermal printer
    /// </summary>
    /// <returns></returns>
    Task DisconnectAsync();

    /// <summary>
    /// Sends a print job command (raw byte data) to the thermal printer
    /// </summary>
    /// <param name="rawData"></param>        
    Task PrintAsync(byte[] rawData, CancellationToken ct = default);


    /// <summary>
    /// Downloads a font file to the thermal printer memory.
    /// </summary>
    /// <param name="fontName">Target filename on printer (e.g. "ROMAN.TTF").</param>
    /// <param name="fontData">Font binary bytes.</param>
    /// <param name="target">Target memory location (Default: Flash memory).</param>
    Task DownloadFontAsync( string fontName, byte[] fontData, MemoryTarget target = MemoryTarget.Flash, CancellationToken ct = default);


    /// <summary>
    /// Downloads a bitmap file to the thermal printer memory.
    /// </summary>
    /// <param name="imageName">Target filename on printer (e.g. "LOGO.BMP").</param>
    /// <param name="bitmapData">Bitmap binary bytes.</param>
    /// <param name="target">Target memory location (Default: Flash memory).</param>
    public Task DownloadBitmapAsync( string imageName, byte[] bitmapData, MemoryTarget target = MemoryTarget.Flash, CancellationToken ct = default);
}