// src/ThermalPrinter.Core/Transports/NetworkTransport.cs
using System.Net.Sockets;

namespace ThermalPrinter.Core.Transports;

public class NetworkTransport : ITransport
{
    private readonly string _ipAddress;
    private readonly int _port;
    private TcpClient? _client;
    private NetworkStream? _stream;


    /// <inheritdoc />
    public bool IsConnected => _client?.Connected ?? false;


    /// <inheritdoc />    
    public bool SupportsBidirectional { get; set; } = true;


    public NetworkTransport(string ipAddress, int port = 9100, bool supportsBidirectional = true)
    {
        this._ipAddress = ipAddress;
        this._port = port;

        this.SupportsBidirectional = supportsBidirectional;
    }


    /// <inheritdoc />    
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(_ipAddress, _port, cancellationToken);
        _stream = _client.GetStream();
    }


    /// <inheritdoc />    
    public Task SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (_stream == null || !IsConnected)
            throw new InvalidOperationException("Transport is not connected.");

        return _stream.WriteAsync(data, 0, data.Length, cancellationToken);
    }


    /// <inheritdoc />    
    public async Task<byte[]> ReadAsync(int bufferSize, CancellationToken cancellationToken = default)
    {
        if (!SupportsBidirectional)
        {
            throw new NotSupportedException("This transport configuration does not support bidirectional communication.");
        }

        if (_stream == null || !IsConnected)
            throw new InvalidOperationException("Transport is not connected.");

        byte[] buffer = new byte[bufferSize];
        int bytesRead = await _stream.ReadAsync(buffer, 0, bufferSize, cancellationToken);

        byte[] result = new byte[bytesRead];
        Array.Copy(buffer, result, bytesRead);
        return result;
    }


    /// <inheritdoc />    
    public Task DisconnectAsync()
    {
        _stream?.Dispose();
        _client?.Close();
        _client?.Dispose();
        return Task.CompletedTask;
    }


    public void Dispose()
    {
        _stream?.Dispose();
        _client?.Dispose();
    }
}