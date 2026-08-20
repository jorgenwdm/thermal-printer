namespace ThermalPrinter.Core.Exceptions;

public class TransportUnidirectionalException : InvalidOperationException
{
    public TransportUnidirectionalException(string transportName)
        : base($"The current transport '{transportName}' is configured as unidirectional (Send-Only). Read operations are not supported.")
    {
    }
}