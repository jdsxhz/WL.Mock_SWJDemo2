using ScadaTemplate.Infrastructure;

namespace ScadaTemplate.Models;

public sealed class ModbusPoint : ObservableObject
{
    private string name = "新点位";
    private ModbusRegisterArea area = ModbusRegisterArea.HoldingRegister;
    private int address;
    private ModbusDataType dataType = ModbusDataType.UInt16;
    private ModbusByteOrder byteOrder = ModbusByteOrder.BigEndian;
    private ModbusWordOrder wordOrder = ModbusWordOrder.HighWordFirst;
    private double scale = 1;
    private double value;
    private bool canWrite = true;

    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    public ModbusRegisterArea Area
    {
        get => area;
        set
        {
            if (SetProperty(ref area, value) && value == ModbusRegisterArea.InputRegister)
            {
                CanWrite = false;
            }
        }
    }

    public int Address
    {
        get => address;
        set => SetProperty(ref address, value);
    }

    public ModbusDataType DataType
    {
        get => dataType;
        set
        {
            if (SetProperty(ref dataType, value))
            {
                OnPropertyChanged(nameof(RegisterCount));
            }
        }
    }

    public ModbusByteOrder ByteOrder
    {
        get => byteOrder;
        set => SetProperty(ref byteOrder, value);
    }

    public ModbusWordOrder WordOrder
    {
        get => wordOrder;
        set => SetProperty(ref wordOrder, value);
    }

    public double Scale
    {
        get => scale;
        set => SetProperty(ref scale, value == 0 ? 1 : value);
    }

    public double Value
    {
        get => value;
        set => SetProperty(ref this.value, value);
    }

    public bool CanWrite
    {
        get => canWrite;
        set => SetProperty(ref canWrite, value);
    }

    public int RegisterCount => DataType switch
    {
        ModbusDataType.UInt16 or ModbusDataType.Int16 => 1,
        ModbusDataType.UInt32 or ModbusDataType.Int32 or ModbusDataType.Float32 => 2,
        ModbusDataType.Float64 => 4,
        _ => 1
    };
}
