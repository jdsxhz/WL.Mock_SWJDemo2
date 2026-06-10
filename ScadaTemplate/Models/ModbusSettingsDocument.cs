namespace ScadaTemplate.Models;

public sealed class ModbusSettingsDocument
{
    public string Host { get; set; } = "127.0.0.1";

    public int Port { get; set; } = 502;

    public int UnitId { get; set; } = 1;

    public List<ModbusPointSettings> Points { get; set; } = [];
}

public sealed class ModbusPointSettings
{
    public string Name { get; set; } = string.Empty;

    public ModbusRegisterArea Area { get; set; }

    public int Address { get; set; }

    public ModbusDataType DataType { get; set; }

    public ModbusByteOrder ByteOrder { get; set; }

    public ModbusWordOrder WordOrder { get; set; }

    public double Scale { get; set; } = 1;

    public bool CanWrite { get; set; } = true;
}
