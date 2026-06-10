using ScadaTemplate.Models;

namespace ScadaTemplate.Services;

public static class ModbusValueCodec
{
    public static double Decode(IReadOnlyList<ushort> registers, ModbusPoint point)
    {
        var bytes = ToOrderedBytes(registers, point.ByteOrder, point.WordOrder);
        var raw = point.DataType switch
        {
            ModbusDataType.UInt16 => BitConverter.ToUInt16(bytes, 0),
            ModbusDataType.Int16 => BitConverter.ToInt16(bytes, 0),
            ModbusDataType.UInt32 => BitConverter.ToUInt32(bytes, 0),
            ModbusDataType.Int32 => BitConverter.ToInt32(bytes, 0),
            ModbusDataType.Float32 => BitConverter.ToSingle(bytes, 0),
            ModbusDataType.Float64 => BitConverter.ToDouble(bytes, 0),
            _ => 0d
        };

        return Convert.ToDouble(raw) * point.Scale;
    }

    public static ushort[] Encode(double engineeringValue, ModbusPoint point)
    {
        var rawValue = engineeringValue / (point.Scale == 0 ? 1 : point.Scale);
        var bytes = point.DataType switch
        {
            ModbusDataType.UInt16 => BitConverter.GetBytes(checked((ushort)Math.Round(rawValue))),
            ModbusDataType.Int16 => BitConverter.GetBytes(checked((short)Math.Round(rawValue))),
            ModbusDataType.UInt32 => BitConverter.GetBytes(checked((uint)Math.Round(rawValue))),
            ModbusDataType.Int32 => BitConverter.GetBytes(checked((int)Math.Round(rawValue))),
            ModbusDataType.Float32 => BitConverter.GetBytes((float)rawValue),
            ModbusDataType.Float64 => BitConverter.GetBytes(rawValue),
            _ => throw new NotSupportedException($"不支持的数据类型：{point.DataType}")
        };

        return FromOrderedBytes(bytes, point.ByteOrder, point.WordOrder);
    }

    private static byte[] ToOrderedBytes(IReadOnlyList<ushort> registers, ModbusByteOrder byteOrder, ModbusWordOrder wordOrder)
    {
        var words = registers.ToArray();
        if (wordOrder == ModbusWordOrder.LowWordFirst && words.Length > 1)
        {
            Array.Reverse(words);
        }

        var bytes = new byte[words.Length * 2];
        for (var i = 0; i < words.Length; i++)
        {
            var high = (byte)(words[i] >> 8);
            var low = (byte)words[i];
            bytes[i * 2] = byteOrder == ModbusByteOrder.BigEndian ? high : low;
            bytes[i * 2 + 1] = byteOrder == ModbusByteOrder.BigEndian ? low : high;
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return bytes;
    }

    private static ushort[] FromOrderedBytes(byte[] source, ModbusByteOrder byteOrder, ModbusWordOrder wordOrder)
    {
        var bytes = source.ToArray();
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        var words = new ushort[bytes.Length / 2];
        for (var i = 0; i < words.Length; i++)
        {
            var first = bytes[i * 2];
            var second = bytes[i * 2 + 1];
            words[i] = byteOrder == ModbusByteOrder.BigEndian
                ? (ushort)((first << 8) | second)
                : (ushort)((second << 8) | first);
        }

        if (wordOrder == ModbusWordOrder.LowWordFirst && words.Length > 1)
        {
            Array.Reverse(words);
        }

        return words;
    }
}
