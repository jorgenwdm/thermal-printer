using ThermalPrinter.Core.Enums;

namespace ThermalPrinter.Core.Transports;

/// <summary>
/// Interface for communication with a thermal printer (or print server attached to a thermal printer)
/// </summary>
public interface ITransport : IDisposable
{    
    /// <summary>
    /// Indicates whether the transport is currently connected to the printer or print server.
    /// </summary>
    bool IsConnected { get; }
    
    
    /// <summary>
    /// Indicates whether the transport supports bidirectional communication (i.e., reading responses from the printer).
    /// If true, the transport can read data from the printer; if false, it can only send data to the printer.
    /// </summary>
    bool SupportsBidirectional { get; set; }
    
    
    /// <summary>
    /// Establishes a connection to the printer or print server.     
    /// </summary>    
    Task ConnectAsync(CancellationToken cancellationToken = default);
    
    
    /// <summary>
    /// Closes the connection to the printer or print server and releases any associated resources.
    /// </summary>    
    Task DisconnectAsync();
    
    
    /// <summary>
    /// Sends raw data to the printer or print server.
    /// </summary>
    /// <param name="data">Raw data in the appropriate format for the printer</param>    
    Task SendAsync(byte[] data, CancellationToken cancellationToken = default);
    

    /// <summary>
    /// Reads raw data from the printer or print server. The behavior of this method depends on the transport's bidirectional support:    
    /// </summary>    
    Task<byte[]> ReadAsync(int bufferSize, CancellationToken cancellationToken = default);
}