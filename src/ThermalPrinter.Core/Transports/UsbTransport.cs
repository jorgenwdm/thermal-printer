using System.IO;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using ThermalPrinter.Core.Exceptions;

namespace ThermalPrinter.Core.Transports;

/// <summary>
/// Direct USB transport implementation using LibUsbDotNet v3 API.
/// Provides cross-platform USB communication for Windows and Linux.
/// </summary>
public class UsbTransport : ITransport
{
    private readonly int _vendorId;
    private readonly int _productId;
    private UsbContext? _context;
    private IUsbDevice? _usbDevice;
    private UsbEndpointWriter? _writer;
    private UsbEndpointReader? _reader;

    /// <inheritdoc />
    public bool IsConnected => _usbDevice != null && _usbDevice.IsOpen;

    /// <inheritdoc />
    public bool SupportsBidirectional { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of <see cref="UsbTransport"/>.
    /// </summary>
    /// <param name="vendorId">USB Vendor ID (VID) in integer/hex format (e.g., 0x1103).</param>
    /// <param name="productId">USB Product ID (PID) in integer/hex format (e.g., 0x0001).</param>
    /// <param name="supportsBidirectional">Indicates whether readback operations are allowed.</param>
    public UsbTransport(int vendorId, int productId, bool supportsBidirectional = true)
    {
        _vendorId = vendorId;
        _productId = productId;
        SupportsBidirectional = supportsBidirectional;
    }


    /// <inheritdoc />
    public Task ConnectAsync(CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            // 1. Initialize UsbContext session (LibUsbDotNet v3)
            _context = new UsbContext();

            // 2. Find device using UsbDeviceFinder
            var finder = new UsbDeviceFinder
            {
                Vid = _vendorId,
                Pid = _productId
            };

            _usbDevice = _context.Find(finder);

            if (_usbDevice == null)
            {
                throw new InvalidOperationException(
                    $"USB Printer Device (VID: 0x{_vendorId:X4}, PID: 0x{_productId:X4}) was not found."
                );
            }

            // 3. Open connection and claim interface
            // Open connection, select configuration #1 and claim interface #0
            _usbDevice.Open();            
            _usbDevice.SetConfiguration(1);
            _usbDevice.ClaimInterface(0);

            // 4. Open Out Endpoint for sending commands
            _writer = _usbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

            // 5. Open In Endpoint if bidirectional communication is enabled
            if (SupportsBidirectional)
            {
                _reader = _usbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
            }
        }, ct);
    }


    /// <inheritdoc />
    public Task DisconnectAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }


    /// <inheritdoc />
    public Task SendAsync(byte[] data, CancellationToken ct = default)
    {
        if (!IsConnected || _writer == null)
        {
            throw new InvalidOperationException("USB Transport is not connected.");
        }

        return Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                // Η Write επιστρέφει LibUsbDotNet.Error
                Error status = _writer.Write(data, 5000, out int bytesWritten);

                if (status != Error.Success || bytesWritten != data.Length)
                {
                    throw new IOException($"USB Send Failure. Status: {status}, Bytes Sent: {bytesWritten}/{data.Length}");
                }

            }
            catch (Exception ex) when (ex is not IOException)
            {
                throw new IOException($"Failed to send raw data over USB: {ex.Message}", ex);
            }
        }, ct);
    }


    /// <inheritdoc />
    public async Task<byte[]> ReadAsync(int bufferSize, CancellationToken ct = default)
    {
        // Fail-fast Transport Guard
        if (!SupportsBidirectional)
        {
            throw new TransportUnidirectionalException(nameof(UsbTransport));
        }

        if (!IsConnected || _reader == null)
        {
            throw new InvalidOperationException("USB Transport is not connected.");
        }

        return await Task.Run(() =>
        {
            byte[] buffer = new byte[bufferSize];

            try
            {
                // Read will return LibUsbDotNet.Error
                Error status = _reader.Read(buffer, 3000, out int bytesRead);

                // Το Error.Timeout αντιμετωπίζεται ως 0 bytes read αν δεν ήρθαν δεδομένα
                if (status != Error.Success && status != Error.Timeout)
                {
                    throw new IOException($"USB Read Failure. Status: {status}");
                }

                if (bytesRead <= 0)
                {
                    return Array.Empty<byte>();
                }

                byte[] result = new byte[bytesRead];
                Array.Copy(buffer, result, bytesRead);
                return result;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to read response over USB: {ex.Message}", ex);
            }

        }, ct);
    }


    public void Dispose()
    {
        if (_usbDevice != null)
        {
            if (_usbDevice.IsOpen)
            {
                _usbDevice.ReleaseInterface(0);
                _usbDevice.Close();
            }
            _usbDevice = null;
        }

        _context?.Dispose();
        _context = null;

        _writer = null;
        _reader = null;
    }
}