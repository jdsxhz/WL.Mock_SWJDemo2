using System.Net.Sockets;
using ScadaTemplate.Models;

namespace ScadaTemplate.Services;

public sealed class ModbusTcpClientService
{
    private ushort transactionId;

    public async Task<ushort[]> ReadRegistersAsync(
        string host,
        int port,
        byte unitId,
        ModbusRegisterArea area,
        ushort startAddress,
        ushort count,
        CancellationToken cancellationToken = default)
    {
        if (count == 0)
        {
            return [];
        }

        var functionCode = area == ModbusRegisterArea.HoldingRegister ? (byte)0x03 : (byte)0x04;
        var request = BuildReadRegistersRequest(unitId, functionCode, startAddress, count);
        var response = await SendAsync(host, port, request, cancellationToken);

        if (response.Length < 9 || response[7] != functionCode)
        {
            throw new InvalidOperationException("Modbus 读取响应无效。");
        }

        var byteCount = response[8];
        if (byteCount != count * 2 || response.Length < 9 + byteCount)
        {
            throw new InvalidOperationException("Modbus 读取长度不匹配。");
        }

        var values = new ushort[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = (ushort)((response[9 + i * 2] << 8) | response[10 + i * 2]);
        }

        return values;
    }

    public async Task WriteMultipleRegistersAsync(
        string host,
        int port,
        byte unitId,
        ushort startAddress,
        IReadOnlyList<ushort> values,
        CancellationToken cancellationToken = default)
    {
        if (values.Count == 0)
        {
            return;
        }

        var request = BuildWriteMultipleRegistersRequest(unitId, startAddress, values);
        var response = await SendAsync(host, port, request, cancellationToken);

        if (response.Length < 12 || response[7] != 0x10)
        {
            throw new InvalidOperationException("Modbus 写入响应无效。");
        }
    }

    public async Task TestConnectionAsync(string host, int port, byte unitId, CancellationToken cancellationToken = default)
    {
        await ReadRegistersAsync(host, port, unitId, ModbusRegisterArea.HoldingRegister, 0, 1, cancellationToken);
    }

    private byte[] BuildReadRegistersRequest(byte unitId, byte functionCode, ushort startAddress, ushort count)
    {
        var id = NextTransactionId();
        return
        [
            (byte)(id >> 8), (byte)id,
            0x00, 0x00,
            0x00, 0x06,
            unitId,
            functionCode,
            (byte)(startAddress >> 8), (byte)startAddress,
            (byte)(count >> 8), (byte)count
        ];
    }

    private byte[] BuildWriteMultipleRegistersRequest(byte unitId, ushort startAddress, IReadOnlyList<ushort> values)
    {
        var id = NextTransactionId();
        var byteCount = checked((byte)(values.Count * 2));
        var length = checked((ushort)(7 + byteCount));
        var request = new byte[13 + byteCount];

        request[0] = (byte)(id >> 8);
        request[1] = (byte)id;
        request[4] = (byte)(length >> 8);
        request[5] = (byte)length;
        request[6] = unitId;
        request[7] = 0x10;
        request[8] = (byte)(startAddress >> 8);
        request[9] = (byte)startAddress;
        request[10] = (byte)(values.Count >> 8);
        request[11] = (byte)values.Count;
        request[12] = byteCount;

        for (var i = 0; i < values.Count; i++)
        {
            request[13 + i * 2] = (byte)(values[i] >> 8);
            request[14 + i * 2] = (byte)values[i];
        }

        return request;
    }

    private async Task<byte[]> SendAsync(string host, int port, byte[] request, CancellationToken cancellationToken)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(host, port, cancellationToken);
        await using var stream = client.GetStream();
        await stream.WriteAsync(request, cancellationToken);

        var header = new byte[7];
        await ReadExactAsync(stream, header, cancellationToken);
        var length = (header[4] << 8) | header[5];
        if (length < 1)
        {
            throw new InvalidOperationException("Modbus TCP 帧长度无效。");
        }

        var body = new byte[length - 1];
        await ReadExactAsync(stream, body, cancellationToken);

        var response = new byte[header.Length + body.Length];
        Buffer.BlockCopy(header, 0, response, 0, header.Length);
        Buffer.BlockCopy(body, 0, response, header.Length, body.Length);

        if (response.Length > 8 && (response[7] & 0x80) != 0)
        {
            throw new InvalidOperationException($"Modbus 异常响应：0x{response[8]:X2}");
        }

        return response;
    }

    private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken);
            if (read == 0)
            {
                throw new InvalidOperationException("Modbus 连接已断开。");
            }

            offset += read;
        }
    }

    private ushort NextTransactionId()
    {
        transactionId++;
        return transactionId;
    }
}
