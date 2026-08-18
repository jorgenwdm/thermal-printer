// src/ThermalPrinter.TSC/TscPrinter.cs
using System.Text;
using ThermalPrinter.Core.Enums;
using ThermalPrinter.Core.Interfaces;
using ThermalPrinter.Core.Transports;

namespace ThermalPrinter.TSC;

public class TscPrinter : IThermalPrinter
{
    private readonly ITransport _transport;

    public TscPrinter(ITransport transport)
    {
        _transport = transport;
    }

    public Task ConnectAsync(CancellationToken cancellationToken = default)
        => _transport.ConnectAsync(cancellationToken);

    public Task DisconnectAsync()
        => _transport.DisconnectAsync();


    /// <inheritdoc />
    public async Task<PrinterStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        // Note: We don't need to check if transport supports bidirectional here. ReadAsync will throw exception if not supported.

        // TSPL Status Command: <ESC>!?
        byte[] command = [(byte)0x1B, (byte)'!', (byte)'?'];
        await _transport.SendAsync(command, cancellationToken);

        byte[] response = await _transport.ReadAsync(1, cancellationToken);
        if (response.Length == 0) return PrinterStatus.Offline;

        byte statusByte = response[0];
        PrinterStatus status = PrinterStatus.Ready;

        if ((statusByte & 0x01) != 0) status |= PrinterStatus.CoverOpen;
        if ((statusByte & 0x02) != 0) status |= PrinterStatus.PaperOut;
        if ((statusByte & 0x04) != 0) status |= PrinterStatus.RibbonEnd;
        if ((statusByte & 0x08) != 0) status |= PrinterStatus.Pause;
        if ((statusByte & 0x10) != 0) status |= PrinterStatus.Error;

        return status;
    }

    /// <inheritdoc />
    public async Task<List<string>> GetFilesAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<string>();

        // Note: We don't need to check if transport supports bidirectional here. ReadAsync will throw exception if not supported.

        // TSPL Get Files Command: <ESC>!?
        byte[] command = [(byte)0x1B, (byte)'!', (byte)'F'];
        await _transport.SendAsync(command, cancellationToken);

        // Buffer for the collection of all the bytes contained in the response
        using var ms = new MemoryStream();
        
        // We define a short timeout per read, so that if the printer stops sending back data, the reading will be completed
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromMilliseconds(1500));

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                byte[] chunk = await _transport.ReadAsync(256, cts.Token);
                if (chunk.Length == 0) break;

                ms.Write(chunk, 0, chunk.Length);
                
                // If the printer sends the EOF (0x1A) or NULL (0x00) this means that the list is completed
                if (Array.IndexOf(chunk, (byte)0x1A) != -1 || Array.IndexOf(chunk, (byte)0x00) != -1)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {            
            // Expected reading timeout when printer completes the successful sending of bytes
        }

        byte[] responseBytes = ms.ToArray();
        if (responseBytes.Length == 0) return result;
        
        // Convert response to an ASCII string
        string rawResponse = Encoding.ASCII.GetString(responseBytes);
        
        // Line parsing and delimiting with \r\n or \n
        string[] lines = rawResponse.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string trimmed = line.Trim('\0', '\x1A', ' ');
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                result.Add(trimmed);
            }
        }

        return result;
    }

    public Task PrintRawAsync(byte[] rawData, CancellationToken cancellationToken = default)
        => _transport.SendAsync(rawData, cancellationToken);


    public Task DownloadFontAsync(string fontName, byte[] fontData, CancellationToken cancellationToken = default)
    {
        string header = $"DOWNLOAD \"{fontName}\",{fontData.Length},";
        byte[] headerBytes = Encoding.ASCII.GetBytes(header);

        byte[] payload = new byte[headerBytes.Length + fontData.Length];
        Buffer.BlockCopy(headerBytes, 0, payload, 0, headerBytes.Length);
        Buffer.BlockCopy(fontData, 0, payload, headerBytes.Length, fontData.Length);

        return _transport.SendAsync(payload, cancellationToken);
    }


    public Task DownloadBitmapAsync(string imageName, byte[] bitmapData, CancellationToken cancellationToken = default)
    {
        string header = $"DOWNLOAD \"{imageName}\",{bitmapData.Length},";
        byte[] headerBytes = Encoding.ASCII.GetBytes(header);

        byte[] payload = new byte[headerBytes.Length + bitmapData.Length];
        Buffer.BlockCopy(headerBytes, 0, payload, 0, headerBytes.Length);
        Buffer.BlockCopy(bitmapData, 0, payload, headerBytes.Length, bitmapData.Length);

        return _transport.SendAsync(payload, cancellationToken);
    }

}