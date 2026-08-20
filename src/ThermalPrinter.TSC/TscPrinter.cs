using System.Text;
using ThermalPrinter.Core.Enums;
using ThermalPrinter.Core.Interfaces;
using ThermalPrinter.Core.Transports;

namespace ThermalPrinter.TSC;

public class TscPrinter : IBidirectionalThermalPrinter
{
    private readonly ITransport _transport;
    
    public TscPrinter(ITransport transport)
    {
        _transport = transport;
       
    }

#region Command Methods

    /// <inheritdoc />
    public Task ConnectAsync(CancellationToken ct = default) => _transport.ConnectAsync(ct);


    /// <inheritdoc />
    public Task DisconnectAsync() => _transport.DisconnectAsync();

    /// <inheritdoc />
    public Task PrintAsync(byte[] rawData, CancellationToken ct = default)
        => _transport.SendAsync(rawData, ct);


    /// <inheritdoc />
    public Task DownloadFontAsync(string fontName, byte[] fontData, MemoryTarget target = MemoryTarget.Flash, CancellationToken ct = default)
    {
        return DownloadFileAsync(fontName, fontData, target, ct);
    }


    /// <inheritdoc />
    public Task DownloadBitmapAsync(string imageName, byte[] bitmapData, MemoryTarget target = MemoryTarget.Flash, CancellationToken ct = default)
    {
        return DownloadFileAsync(imageName, bitmapData, target, ct);
    }


    // Helper method for TSPL DOWNLOAD [n,] "FILENAME",DATA SIZE,DATA CONTENT...    
    private Task DownloadFileAsync(string fileName, byte[] fileData, MemoryTarget target,CancellationToken ct)
    {
        string memoryPrefix = target switch
        {
            MemoryTarget.Flash => "F,",
            MemoryTarget.Expansion => "E,",
            _ => string.Empty // DRAM
        };

        // Command Syntax: DOWNLOAD [n,] "FILENAME",DATA SIZE,
        string header = $"DOWNLOAD {memoryPrefix}\"{fileName}\",{fileData.Length},";
        byte[] headerBytes = Encoding.ASCII.GetBytes(header);

        // Terminating CRLF after binary payload
        byte[] footerBytes = Encoding.ASCII.GetBytes("\r\n");

        byte[] payload = new byte[headerBytes.Length + fileData.Length + footerBytes.Length];

        Buffer.BlockCopy(headerBytes, 0, payload, 0, headerBytes.Length);
        Buffer.BlockCopy(fileData, 0, payload, headerBytes.Length, fileData.Length);
        Buffer.BlockCopy(footerBytes, 0, payload, headerBytes.Length + fileData.Length, footerBytes.Length);

        return _transport.SendAsync(payload, ct);
    }

#endregion


#region Query Methods

    /// <inheritdoc />    
    public async Task<PrinterStatus> GetStatusAsync(CancellationToken ct = default)
    {
        using var cancellationToken = ct == default
            ? new CancellationTokenSource(TimeSpan.FromSeconds(15))
            : CancellationTokenSource.CreateLinkedTokenSource(ct);

        // TSPL Status Command: <ESC>!?
        byte[] command = [(byte)0x1B, (byte)'!', (byte)'?'];
        await _transport.SendAsync(command, cancellationToken.Token);

        byte[] response = await _transport.ReadAsync(1, cancellationToken.Token);
        if (response.Length == 0) return PrinterStatus.Offline;

        byte statusByte = response[0];

        if (statusByte == 0x00) return PrinterStatus.Ready;

        PrinterStatus status = PrinterStatus.Ready;

        // Bitwise evaluation based on TSPL Spec Table
        if ((statusByte & 0x01) != 0) status |= PrinterStatus.CoverOpen;  // Head opened
        if ((statusByte & 0x02) != 0) status |= PrinterStatus.Error;      // Paper Jam (mapped to Error)
        if ((statusByte & 0x04) != 0) status |= PrinterStatus.PaperOut;   // Out of paper
        if ((statusByte & 0x08) != 0) status |= PrinterStatus.RibbonEnd;  // Out of ribbon
        if ((statusByte & 0x10) != 0) status |= PrinterStatus.Pause;      // Pause
        if ((statusByte & 0x80) != 0) status |= PrinterStatus.Error;      // Other error

        return status;
    }


    /// <inheritdoc />    
    public async Task<List<string>> GetFilesAsync(CancellationToken ct = default)
    {
        var result = new List<string>();

        // TSPL File Listing Command: ~!F
        byte[] command = Encoding.ASCII.GetBytes("~!F\r\n");
        await _transport.SendAsync(command, ct);

        using var ms = new MemoryStream();

        using var overallCts = ct == default
            ? new CancellationTokenSource(TimeSpan.FromSeconds(15))
            : CancellationTokenSource.CreateLinkedTokenSource(ct);

        try
        {
            while (!overallCts.Token.IsCancellationRequested) // while cancelation has not been requested yet
            {
                byte[] chunk = await _transport.ReadAsync(256, overallCts.Token); // read the next chunk of the response

                if (chunk.Length > 0)
                {
                    ms.Write(chunk, 0, chunk.Length);   // write it to the memory stream

                    // Check for EOF (0x1A) or NULL (0x00) termination markers
                    if (Array.IndexOf(chunk, (byte)0x1A) != -1 || Array.IndexOf(chunk, (byte)0x00) != -1) break;
                }
                else
                {
                    // If no data arrived yet, wait 200ms before checking the socket again
                    await Task.Delay(200, overallCts.Token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when overall timeout (15s) expires
        }

        // Join all the chunks into a final response, convert it to string and split it per line
        byte[] responseBytes = ms.ToArray();
        if (responseBytes.Length == 0) return result;

        string rawResponse = Encoding.ASCII.GetString(responseBytes);
        string[] lines = rawResponse.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        // Every line (trimmed) will be added to the result list of strings
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

#endregion

}