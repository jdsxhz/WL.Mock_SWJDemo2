using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ScadaTemplate.Infrastructure;
using ScadaTemplate.Models;
using ScadaTemplate.Services;

namespace ScadaTemplate.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly DispatcherTimer timer;
    private readonly Random random = new();
    private readonly ModbusTcpClientService modbus = new();
    private string selectedDevice = "设备名称1";
    private string currentTime = string.Empty;
    private string operationMessage = "系统待机，等待启动";
    private bool isRunning;
    private double parameterA = 120;
    private double parameterB = 80;
    private double tankTemperature1 = 14;
    private double tankTemperature2 = 14;
    private double inletTemperature = 14;
    private double outletTemperature = 14;
    private double filterLevel1 = 74;
    private double filterLevel2 = 68;
    private bool valve1Open;
    private bool valve2Open;
    private bool pump1Running;
    private bool pump2Running;
    private string currentPage = "Overview";
    private string modbusHost = "127.0.0.1";
    private int modbusPort = 502;
    private int modbusUnitId = 1;
    private int parameterStartAddress;
    private bool isModbusBusy;
    private ModbusPoint? selectedModbusPoint;
    private int selectedTrendHours = 1;

    public MainViewModel()
    {
        CurrentTime = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss dddd");

        SeedDevices();
        SeedMessages();
        SeedOptions();
        SeedParameters();
        SeedModbusPoints();
        LoadSettings(showResult: false);

        StartCommand = new RelayCommand(_ => StartSystem(), _ => !IsRunning);
        ResetCommand = new RelayCommand(_ => ResetSystem());
        StopCommand = new RelayCommand(_ => StopSystem(), _ => IsRunning);
        ApplyParametersCommand = new AsyncRelayCommand(_ => WriteParametersAsync(), _ => !IsModbusBusy);
        ReadParametersCommand = new AsyncRelayCommand(_ => ReadParametersAsync(), _ => !IsModbusBusy);
        TestConnectionCommand = new AsyncRelayCommand(_ => TestConnectionAsync(), _ => !IsModbusBusy);
        AcknowledgeAlarmCommand = new RelayCommand(_ => AcknowledgeAlarm(), _ => Messages.Count > 0);
        AcknowledgeSingleAlarmCommand = new RelayCommand(AcknowledgeSingleAlarm, parameter => parameter is MessageItem);
        NavigateCommand = new RelayCommand(parameter => Navigate(parameter?.ToString()));
        ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
        MinimizeCommand = new RelayCommand(_ => SetWindowState(WindowState.Minimized));
        ToggleMaximizeCommand = new RelayCommand(_ => ToggleWindowState());
        SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
        ExportTrendCommand = new RelayCommand(_ => ExportTrend());
        CreateWorkOrderCommand = new RelayCommand(_ => CreateWorkOrder());
        AddModbusPointCommand = new RelayCommand(_ => AddModbusPoint());
        RemoveModbusPointCommand = new RelayCommand(_ => RemoveModbusPoint(), _ => SelectedModbusPoint is not null);
        SelectTrendRangeCommand = new RelayCommand(parameter => SelectTrendRange(parameter));
        ReloadSettingsCommand = new RelayCommand(_ => LoadSettings(showResult: true));

        timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => RefreshRuntimeData();
        timer.Start();
    }

    public ObservableCollection<DeviceStatus> Devices { get; } = [];

    public ObservableCollection<MessageItem> Messages { get; } = [];

    public ObservableCollection<OptionItem> Switches { get; } = [];

    public ObservableCollection<OptionItem> Choices { get; } = [];

    public ObservableCollection<ParameterItem> Parameters { get; } = [];

    public ObservableCollection<ModbusPoint> ModbusPoints { get; } = [];

    public Array RegisterAreas { get; } = Enum.GetValues<ModbusRegisterArea>();

    public Array ModbusDataTypes { get; } = Enum.GetValues<ModbusDataType>();

    public Array ByteOrders { get; } = Enum.GetValues<ModbusByteOrder>();

    public Array WordOrders { get; } = Enum.GetValues<ModbusWordOrder>();

    public string[] DeviceOptions { get; } =
    [
        "设备名称1",
        "设备名称2",
        "设备名称3"
    ];

    public string SelectedDevice
    {
        get => selectedDevice;
        set => SetProperty(ref selectedDevice, value);
    }

    public string CurrentTime
    {
        get => currentTime;
        set => SetProperty(ref currentTime, value);
    }

    public string OperationMessage
    {
        get => operationMessage;
        set => SetProperty(ref operationMessage, value);
    }

    public bool IsRunning
    {
        get => isRunning;
        set
        {
            if (SetProperty(ref isRunning, value))
            {
                OnPropertyChanged(nameof(SystemStateText));
                OnPropertyChanged(nameof(SystemStateStatus));
                (StartCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (StopCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string SystemStateText => IsRunning ? "运行中" : "已停止";

    public StatusKind SystemStateStatus => IsRunning ? StatusKind.Normal : StatusKind.Offline;

    public double ParameterA
    {
        get => parameterA;
        set => SetProperty(ref parameterA, value);
    }

    public double ParameterB
    {
        get => parameterB;
        set => SetProperty(ref parameterB, value);
    }

    public double TankTemperature1
    {
        get => tankTemperature1;
        set => SetProperty(ref tankTemperature1, value);
    }

    public double TankTemperature2
    {
        get => tankTemperature2;
        set => SetProperty(ref tankTemperature2, value);
    }

    public double InletTemperature
    {
        get => inletTemperature;
        set => SetProperty(ref inletTemperature, value);
    }

    public double OutletTemperature
    {
        get => outletTemperature;
        set => SetProperty(ref outletTemperature, value);
    }

    public double FilterLevel1
    {
        get => filterLevel1;
        set => SetProperty(ref filterLevel1, value);
    }

    public double FilterLevel2
    {
        get => filterLevel2;
        set => SetProperty(ref filterLevel2, value);
    }

    public bool Valve1Open
    {
        get => valve1Open;
        set => SetProperty(ref valve1Open, value);
    }

    public bool Valve2Open
    {
        get => valve2Open;
        set => SetProperty(ref valve2Open, value);
    }

    public bool Pump1Running
    {
        get => pump1Running;
        set => SetProperty(ref pump1Running, value);
    }

    public bool Pump2Running
    {
        get => pump2Running;
        set => SetProperty(ref pump2Running, value);
    }

    public string ModbusHost
    {
        get => modbusHost;
        set => SetProperty(ref modbusHost, value);
    }

    public int ModbusPort
    {
        get => modbusPort;
        set => SetProperty(ref modbusPort, value);
    }

    public int ModbusUnitId
    {
        get => modbusUnitId;
        set => SetProperty(ref modbusUnitId, value);
    }

    public int ParameterStartAddress
    {
        get => parameterStartAddress;
        set => SetProperty(ref parameterStartAddress, value);
    }

    public bool IsModbusBusy
    {
        get => isModbusBusy;
        set
        {
            if (SetProperty(ref isModbusBusy, value))
            {
                (ApplyParametersCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (ReadParametersCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (TestConnectionCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public ModbusPoint? SelectedModbusPoint
    {
        get => selectedModbusPoint;
        set
        {
            if (SetProperty(ref selectedModbusPoint, value))
            {
                (RemoveModbusPointCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public int SelectedTrendHours
    {
        get => selectedTrendHours;
        set
        {
            if (SetProperty(ref selectedTrendHours, value))
            {
                OnPropertyChanged(nameof(TrendRangeText));
            }
        }
    }

    public string TrendRangeText => $"当前时间范围：最近 {SelectedTrendHours} 小时";

    public ICommand StartCommand { get; }

    public ICommand ResetCommand { get; }

    public ICommand StopCommand { get; }

    public ICommand ApplyParametersCommand { get; }

    public ICommand ReadParametersCommand { get; }

    public ICommand TestConnectionCommand { get; }

    public ICommand AcknowledgeAlarmCommand { get; }

    public ICommand AcknowledgeSingleAlarmCommand { get; }

    public ICommand NavigateCommand { get; }

    public ICommand ExitCommand { get; }

    public ICommand MinimizeCommand { get; }

    public ICommand ToggleMaximizeCommand { get; }

    public ICommand SaveSettingsCommand { get; }

    public ICommand ExportTrendCommand { get; }

    public ICommand CreateWorkOrderCommand { get; }

    public ICommand AddModbusPointCommand { get; }

    public ICommand RemoveModbusPointCommand { get; }

    public ICommand SelectTrendRangeCommand { get; }

    public ICommand ReloadSettingsCommand { get; }

    public string CurrentPage
    {
        get => currentPage;
        set
        {
            if (SetProperty(ref currentPage, value))
            {
                OnPropertyChanged(nameof(PageTitle));
                OnPropertyChanged(nameof(PageDescription));
            }
        }
    }

    public string PageTitle => CurrentPage switch
    {
        "Overview" => "监控总览",
        "Parameters" => "参数配置",
        "Alarms" => "报警记录",
        "Trends" => "趋势曲线",
        "Maintenance" => "设备维护",
        "Settings" => "系统设置",
        _ => "监控总览"
    };

    public string PageDescription => CurrentPage switch
    {
        "Overview" => "查看流程状态、实时报警、设备运行和开关量。",
        "Parameters" => "通过 Modbus TCP 读取和下发工艺参数。",
        "Alarms" => "确认当前报警，确认后报警从列表中移除。",
        "Trends" => "查看温度、液位、泵频率等过程量趋势。",
        "Maintenance" => "查看设备维护状态、保养周期和联锁条件。",
        "Settings" => "配置 Modbus 通讯、用户权限和系统策略。",
        _ => string.Empty
    };

    private void SeedDevices()
    {
        var statusCycle = new[]
        {
            StatusKind.Error, StatusKind.Offline, StatusKind.Error, StatusKind.Normal, StatusKind.Error, StatusKind.Error,
            StatusKind.Error, StatusKind.Error, StatusKind.Warning, StatusKind.Error, StatusKind.Error, StatusKind.Error,
            StatusKind.Error, StatusKind.Error, StatusKind.Normal, StatusKind.Error, StatusKind.Warning, StatusKind.Error
        };

        for (var i = 0; i < statusCycle.Length; i++)
        {
            Devices.Add(new DeviceStatus($"设备名称{i + 1}", statusCycle[i], i % 3 == 0 ? "通讯正常" : "待确认", 20 + i));
        }
    }

    private void SeedMessages()
    {
        Messages.Add(new MessageItem(StatusKind.Error, DateTime.Now.AddMinutes(-8), "泵站1", "反馈异常"));
        Messages.Add(new MessageItem(StatusKind.Warning, DateTime.Now.AddMinutes(-5), "过滤器1", "液位偏高"));
        Messages.Add(new MessageItem(StatusKind.Warning, DateTime.Now.AddMinutes(-3), "出口温度", "接近阈值"));
    }

    private void SeedOptions()
    {
        for (var i = 0; i < 12; i++)
        {
            Switches.Add(new OptionItem($"开关量{i + 1:00}", i % 4 == 0 ? StatusKind.Normal : StatusKind.Error));
        }

        Choices.Add(new OptionItem("手动模式", StatusKind.Offline));
        Choices.Add(new OptionItem("自动模式", StatusKind.Warning));
        Choices.Add(new OptionItem("远程允许", StatusKind.Warning));
        Choices.Add(new OptionItem("急停回路", StatusKind.Offline));
        Choices.Add(new OptionItem("泵1联锁", StatusKind.Offline));
        Choices.Add(new OptionItem("泵2联锁", StatusKind.Offline));
    }

    private void SeedParameters()
    {
        Parameters.Add(new ParameterItem("进料高度", 120, "CM", 0, 200));
        Parameters.Add(new ParameterItem("回流高度", 80, "CM", 0, 200));
        Parameters.Add(new ParameterItem("温度上限", 35, "°C", 0, 80));
        Parameters.Add(new ParameterItem("泵1频率", 42, "Hz", 0, 50));
        Parameters.Add(new ParameterItem("泵2频率", 39, "Hz", 0, 50));
        Parameters.Add(new ParameterItem("阀门开度", 65, "%", 0, 100));
        Parameters.Add(new ParameterItem("采样周期", 10, "s", 1, 60));
        Parameters.Add(new ParameterItem("报警延时", 3, "s", 0, 20));
    }

    private void SeedModbusPoints()
    {
        ModbusPoints.Add(new ModbusPoint { Name = "进料高度", Address = 0, DataType = ModbusDataType.UInt16, Scale = 0.1, Value = 120 });
        ModbusPoints.Add(new ModbusPoint { Name = "回流高度", Address = 1, DataType = ModbusDataType.UInt16, Scale = 0.1, Value = 80 });
        ModbusPoints.Add(new ModbusPoint { Name = "温度上限", Address = 2, DataType = ModbusDataType.Int16, Scale = 0.1, Value = 35 });
        ModbusPoints.Add(new ModbusPoint { Name = "泵1频率", Address = 3, DataType = ModbusDataType.Float32, Scale = 1, Value = 42 });
        ModbusPoints.Add(new ModbusPoint { Name = "泵2频率", Address = 5, DataType = ModbusDataType.Float32, Scale = 1, Value = 39 });
        ModbusPoints.Add(new ModbusPoint { Name = "阀门开度", Address = 7, DataType = ModbusDataType.UInt16, Scale = 0.1, Value = 65 });
        ModbusPoints.Add(new ModbusPoint { Name = "入口温度", Area = ModbusRegisterArea.InputRegister, Address = 0, DataType = ModbusDataType.Float32, CanWrite = false });
        ModbusPoints.Add(new ModbusPoint { Name = "出口温度", Area = ModbusRegisterArea.InputRegister, Address = 2, DataType = ModbusDataType.Float32, CanWrite = false });
        SelectedModbusPoint = ModbusPoints.FirstOrDefault();
    }

    private void StartSystem()
    {
        IsRunning = true;
        Valve1Open = true;
        Valve2Open = true;
        Pump1Running = true;
        Pump2Running = true;
        OperationMessage = "系统已启动，流程设备进入自动运行";

        foreach (var item in Choices)
        {
            item.Status = StatusKind.Warning;
        }
    }

    private void StopSystem()
    {
        IsRunning = false;
        Valve1Open = false;
        Valve2Open = false;
        Pump1Running = false;
        Pump2Running = false;
        OperationMessage = "系统已停止，泵阀输出关闭";

        foreach (var item in Choices)
        {
            item.Status = StatusKind.Offline;
        }
    }

    private void ResetSystem()
    {
        Messages.Clear();
        SeedMessages();
        OperationMessage = "系统复位完成，报警列表已刷新";
        (AcknowledgeAlarmCommand as RelayCommand)?.RaiseCanExecuteChanged();

        foreach (var device in Devices.Where((_, index) => index % 5 == 0))
        {
            device.Status = StatusKind.Normal;
        }
    }

    private async Task ReadParametersAsync()
    {
        await ExecuteModbusAsync(async () =>
        {
            foreach (var point in ModbusPoints)
            {
                var registers = await modbus.ReadRegistersAsync(
                    ModbusHost,
                    ModbusPort,
                    checked((byte)ModbusUnitId),
                    point.Area,
                    checked((ushort)point.Address),
                    checked((ushort)point.RegisterCount));

                point.Value = ModbusValueCodec.Decode(registers, point);
            }

            SyncParametersFromPoints();
            OperationMessage = $"已从 Modbus TCP 读取 {ModbusPoints.Count} 个点位";
        });
    }

    private async Task WriteParametersAsync()
    {
        await ExecuteModbusAsync(async () =>
        {
            SyncPointsFromParameters();
            var writablePoints = ModbusPoints
                .Where(point => point.CanWrite && point.Area == ModbusRegisterArea.HoldingRegister)
                .ToArray();

            foreach (var point in writablePoints)
            {
                var registers = ModbusValueCodec.Encode(point.Value, point);
                await modbus.WriteMultipleRegistersAsync(
                    ModbusHost,
                    ModbusPort,
                    checked((byte)ModbusUnitId),
                    checked((ushort)point.Address),
                    registers);
            }

            OperationMessage = $"已通过 Modbus TCP 下发 {writablePoints.Length} 个可写点位";
        });
    }

    private async Task TestConnectionAsync()
    {
        await ExecuteModbusAsync(async () =>
        {
            var point = ModbusPoints.FirstOrDefault();
            if (point is null)
            {
                throw new InvalidOperationException("请先配置至少一个 Modbus 点位。");
            }

            await modbus.ReadRegistersAsync(
                ModbusHost,
                ModbusPort,
                checked((byte)ModbusUnitId),
                point.Area,
                checked((ushort)point.Address),
                checked((ushort)point.RegisterCount));
            OperationMessage = $"Modbus TCP 连接成功：{ModbusHost}:{ModbusPort}，站号 {ModbusUnitId}";
        });
    }

    private async Task ExecuteModbusAsync(Func<Task> action)
    {
        try
        {
            IsModbusBusy = true;
            await action();
        }
        catch (Exception ex)
        {
            OperationMessage = $"Modbus TCP 操作失败：{ex.Message}";
        }
        finally
        {
            IsModbusBusy = false;
        }
    }

    private void SyncParametersFromPoints()
    {
        for (var i = 0; i < Parameters.Count && i < ModbusPoints.Count; i++)
        {
            Parameters[i].Value = ModbusPoints[i].Value;
        }

        ParameterA = Parameters.ElementAtOrDefault(0)?.Value ?? ParameterA;
        ParameterB = Parameters.ElementAtOrDefault(1)?.Value ?? ParameterB;
        InletTemperature = ModbusPoints.FirstOrDefault(point => point.Name == "入口温度")?.Value ?? InletTemperature;
        OutletTemperature = ModbusPoints.FirstOrDefault(point => point.Name == "出口温度")?.Value ?? OutletTemperature;
    }

    private void SyncPointsFromParameters()
    {
        for (var i = 0; i < Parameters.Count && i < ModbusPoints.Count; i++)
        {
            ModbusPoints[i].Value = Parameters[i].Value;
        }
    }

    private void AddModbusPoint()
    {
        var nextAddress = ModbusPoints.Count == 0
            ? 0
            : ModbusPoints.Max(point => point.Address + point.RegisterCount);
        var point = new ModbusPoint
        {
            Name = $"点位{ModbusPoints.Count + 1}",
            Address = nextAddress
        };

        ModbusPoints.Add(point);
        SelectedModbusPoint = point;
        OperationMessage = "已新增 Modbus 点位";
    }

    private void RemoveModbusPoint()
    {
        if (SelectedModbusPoint is null)
        {
            return;
        }

        ModbusPoints.Remove(SelectedModbusPoint);
        SelectedModbusPoint = ModbusPoints.FirstOrDefault();
        OperationMessage = "已删除选中的 Modbus 点位";
    }

    private void SelectTrendRange(object? parameter)
    {
        if (int.TryParse(parameter?.ToString(), out var hours) && hours > 0)
        {
            SelectedTrendHours = hours;
            OperationMessage = $"趋势时间范围已切换为最近 {hours} 小时";
        }
    }

    private void ExportTrend()
    {
        var directory = EnsureOutputDirectory("Exports");
        var path = Path.Combine(directory, $"趋势数据_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var builder = new StringBuilder();
        builder.AppendLine("时间范围,点位名称,寄存器区,地址,数据类型,当前值,倍率");

        foreach (var point in ModbusPoints)
        {
            builder.AppendLine($"{SelectedTrendHours}小时,{EscapeCsv(point.Name)},{point.Area},{point.Address},{point.DataType},{point.Value:0.###},{point.Scale:0.###}");
        }

        File.WriteAllText(path, builder.ToString(), new UTF8Encoding(true));
        OperationMessage = $"趋势数据已导出：{path}";
    }

    private void CreateWorkOrder()
    {
        var directory = EnsureOutputDirectory("WorkOrders");
        var path = Path.Combine(directory, $"维护工单_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        var targets = Devices.Where(device => device.Status != StatusKind.Normal).ToArray();
        var lines = new List<string>
        {
            "设备维护工单",
            $"生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            $"设备数量：{targets.Length}",
            string.Empty
        };

        lines.AddRange(targets.Select(device => $"{device.Name}\t{device.StatusText}\t{device.Description}\t当前值 {device.Value:0.##}"));
        File.WriteAllLines(path, lines, new UTF8Encoding(true));
        OperationMessage = $"维护工单已生成：{path}";
    }

    private void SaveSettings()
    {
        var directory = EnsureOutputDirectory("Config");
        var path = Path.Combine(directory, "modbus-settings.json");
        var document = new ModbusSettingsDocument
        {
            Host = ModbusHost,
            Port = ModbusPort,
            UnitId = ModbusUnitId,
            Points = ModbusPoints.Select(point => new ModbusPointSettings
            {
                Name = point.Name,
                Area = point.Area,
                Address = point.Address,
                DataType = point.DataType,
                ByteOrder = point.ByteOrder,
                WordOrder = point.WordOrder,
                Scale = point.Scale,
                CanWrite = point.CanWrite
            }).ToList()
        };

        File.WriteAllText(path, JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true }));
        OperationMessage = $"系统设置已保存：{path}";
    }

    private void LoadSettings(bool showResult)
    {
        try
        {
            var path = Path.Combine(EnsureOutputDirectory("Config"), "modbus-settings.json");
            if (!File.Exists(path))
            {
                if (showResult)
                {
                    OperationMessage = "未找到已保存的 Modbus 配置";
                }

                return;
            }

            var document = JsonSerializer.Deserialize<ModbusSettingsDocument>(File.ReadAllText(path));
            if (document is null)
            {
                throw new InvalidOperationException("配置文件内容为空。");
            }

            ModbusHost = document.Host;
            ModbusPort = document.Port;
            ModbusUnitId = document.UnitId;

            if (document.Points.Count > 0)
            {
                ModbusPoints.Clear();
                foreach (var saved in document.Points)
                {
                    ModbusPoints.Add(new ModbusPoint
                    {
                        Name = saved.Name,
                        Area = saved.Area,
                        Address = saved.Address,
                        DataType = saved.DataType,
                        ByteOrder = saved.ByteOrder,
                        WordOrder = saved.WordOrder,
                        Scale = saved.Scale,
                        CanWrite = saved.CanWrite
                    });
                }

                SelectedModbusPoint = ModbusPoints.FirstOrDefault();
            }

            if (showResult)
            {
                OperationMessage = $"Modbus 配置已重新加载：{path}";
            }
        }
        catch (Exception ex)
        {
            OperationMessage = $"加载 Modbus 配置失败：{ex.Message}";
        }
    }

    private static string EnsureOutputDirectory(string childDirectory)
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ScadaTemplate", childDirectory);
        Directory.CreateDirectory(root);
        return root;
    }

    private static string EscapeCsv(string value)
    {
        return value.Contains(',') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }

    private void AcknowledgeAlarm()
    {
        Messages.Clear();
        OperationMessage = "当前报警已确认并从列表移除";
        (AcknowledgeAlarmCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void AcknowledgeSingleAlarm(object? parameter)
    {
        if (parameter is not MessageItem message)
        {
            return;
        }

        Messages.Remove(message);
        OperationMessage = $"报警已确认：{message.Name} - {message.Content}";
        (AcknowledgeAlarmCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void Navigate(string? page)
    {
        if (string.IsNullOrWhiteSpace(page))
        {
            return;
        }

        CurrentPage = page;
        OperationMessage = $"已切换到{PageTitle}";
    }

    private static void SetWindowState(WindowState state)
    {
        if (Application.Current.MainWindow is { } window)
        {
            window.WindowState = state;
        }
    }

    private static void ToggleWindowState()
    {
        if (Application.Current.MainWindow is not { } window)
        {
            return;
        }

        window.WindowState = window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void RefreshRuntimeData()
    {
        CurrentTime = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss dddd");

        if (!IsRunning)
        {
            return;
        }

        TankTemperature1 = Math.Round(13.5 + random.NextDouble() * 2.2, 1);
        TankTemperature2 = Math.Round(13.2 + random.NextDouble() * 2.5, 1);
        InletTemperature = Math.Round(12.8 + random.NextDouble() * 2.0, 1);
        OutletTemperature = Math.Round(14.2 + random.NextDouble() * 2.8, 1);
        FilterLevel1 = Math.Round(68 + random.NextDouble() * 18, 1);
        FilterLevel2 = Math.Round(58 + random.NextDouble() * 22, 1);

        if (Devices.Count > 0)
        {
            var index = random.Next(Devices.Count);
            Devices[index].Value = Math.Round(10 + random.NextDouble() * 90, 1);
            Devices[index].Status = random.NextDouble() > 0.18 ? StatusKind.Normal : StatusKind.Warning;
        }
    }
}
