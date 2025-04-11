using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using KupaKuper_HmiView.HelpVoid;
using KupaKuper_HmiView.Resources;

using KupaKuper_IO.Ethernet;
using KupaKuper_IO.HelpVoid;

using KupaKuper_MauiControl.ControlModes;

using System.Collections.ObjectModel;

namespace KupaKuper_HmiView.ContentViewModes
{
    // 报警视图的ViewModel类，继承自ObservableObject以支持属性更新通知
    public partial class AlarmViewVM : BaseViewMode
    {
        /// <summary>
        /// 当前选中的日期，默认为今天
        /// </summary>
        [ObservableProperty]
        private DateTime selectedDate = DateTime.Today;

        /// <summary>
        /// 搜索文本
        /// </summary>
        [ObservableProperty]
        private string searchText = string.Empty;

        /// <summary>
        /// 是否只显示报警（不显示提示）
        /// </summary>
        [ObservableProperty]
        private bool showOnlyAlarms = false;

        /// <summary>
        /// 当前选中的过滤类型（全部/报警/提示）
        /// </summary>
        [ObservableProperty]
        private AlarmFilterType selectedFilterType = AlarmFilterType.All;

        /// <summary>
        /// 存储实时报警的可观察集合
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<AlarmRecord> activeAlarms = new();

        /// <summary>
        /// 当前活动报警的数量
        /// </summary>
        [ObservableProperty]
        private int activeAlarmCount;

        /// <summary>
        /// 历史报警显示数据
        /// </summary>
        [ObservableProperty]
        private CsvTableDataMode alarmRecordsData = new();

        /// <summary>
        /// 系统报警变量触发记录
        /// </summary>
        private Int32 _systemAlarm = 0;

        /// <summary>
        /// 上一次读取报警后的记录值
        /// </summary>
        private List<bool> _LastAlarmData = new();

        /// <summary>
        /// 系统提示变量触发记录
        /// </summary>
        private Int32 _systemInfo = 0;

        /// <summary>
        /// 上一次读取提示后的记录值
        /// </summary>
        private List<bool> _LastInfoData = new();
        /// <summary>
        /// 获取当前时间
        /// </summary>
        private string TimeNow
        {
            get
            {
                return DateTime.Now.ToString("yyyy/MM/dd_HH:mm:ss.fff");
            }
        }

        public override uint ViewIndex { get; set; }
        public override string ViewName { get => AppResources.ResourceManager?.GetString("报警监控") ?? "报警监控"; set => throw new NotImplementedException(); }

        /// <summary>
        /// 搜索执行委托
        /// </summary>
        /// <param name="str"></param>
        public delegate void SearchAlarm(string str);
        /// <summary>
        /// 执行搜索方法
        /// </summary>
        public event SearchAlarm SearchAlarmed;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AlarmViewVM()
        {
            System.Diagnostics.Debug.WriteLine("AlarmViewVM 构造函数开始");
            // 初始化时加载当天的报警记录
            _ = LoadAlarmRecords(DateTime.Today);
            //Nlog记录(弃用)
            //_alarmLog = LogManager.GetLogger("AlarmLog");

            // 启动实时报警监控
            StartAlarmMonitoring();
        }

        /// <summary>
        /// 日期改变时的命令处理方法
        /// </summary>
        /// <param name="newDate"></param>
        /// <returns></returns>
        public async Task DateChanged(DateTime newDate)
        {
            SelectedDate = newDate;
            await LoadAlarmRecords(newDate);
            System.Diagnostics.Debug.WriteLine($"日期改变为: {newDate:yyyy-MM-dd}");
        }

        /// <summary>
        /// 搜索报警的命令处理方法
        /// </summary>
        [RelayCommand]
        private void SearchAlarms()
        {
            SearchAlarmed.Invoke(SearchText);
        }

        /// <summary>
        /// 刷新的处理方法
        /// </summary>
        [RelayCommand]
        private async Task UpDataAlarms()
        {
            if (IsDebug)
            {
                AddTestActiveAlarms();
            }
            else
            {
                await LoadAlarmRecords(DateTime.Today);
            }
        }

        /// <summary>
        /// 导出到Excel的命令处理方法
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task ExportToExcel()
        {
            try
            {
                string csvAddress = @".\AlarmLog\" + Device.deviceMessage.DeviecName + @"\" + SelectedDate.ToString("yyyy_MM_dd") + ".csv";
                if (!File.Exists(csvAddress))
                {
                    await DisplayAlertHelp.TryDisplayAlert("提示", "没有找到对应日期的报警记录", "确定");
                    return;
                }

                try
                {
                    // 使用MAUI的文件选择器
                    var location = await FilePicker.PickAsync(new PickOptions
                    {
                        PickerTitle = "选择保存位置",
                        FileTypes = new FilePickerFileType(
                            new Dictionary<DevicePlatform, IEnumerable<string>>
                            {
                        { DevicePlatform.WinUI, new[] { ".xlsx" } },
                        { DevicePlatform.macOS, new[] { "xlsx" } },
                        { DevicePlatform.iOS, new[] { "public.xlsx" } },
                        { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } }
                            })
                    });

                    if (location != null)
                    {
                        string savePath = location.FullPath;
                        if (!savePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                        {
                            savePath += ".xlsx";
                        }

                        // 执行转换
                        ExcelFileHelp.CsvToExcel(csvAddress, savePath);
                        await DisplayAlertHelp.TryDisplayAlert("成功", "报警记录已导出", "确定");
                    }
                }
                catch (PermissionException)
                {
                    // 请求存储权限
                    var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlertHelp.TryDisplayAlert("错误", "需要存储权限才能导出文件", "确定");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"导出Excel时出错: {ex.Message}");
                await DisplayAlertHelp.TryDisplayAlert("错误", "导出Excel文件失败", "确定");
            }
        }
        /// <summary>
        /// 设置过滤类型的命令处理方法
        /// </summary>
        /// <param name="filterType"></param>
        [RelayCommand]
        private void SetFilterType(AlarmFilterType filterType)
        {
            SelectedFilterType = filterType;
            var str = SelectedFilterType == AlarmFilterType.All ? "" : SelectedFilterType.ToString();
            SearchAlarmed.Invoke(str);
            System.Diagnostics.Debug.WriteLine($"过滤类型改变为: {filterType}");
        }

        /// <summary>
        /// 加载指定日期的报警记录
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private async Task LoadAlarmRecords(DateTime date)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"开始加载报警记录，日期：{date:yyyy-MM-dd}");
                //获取报警数据
                List<string[]> records = new();
                records = await LocalAlarmData(date);

                // 用于CsvTable的报警记录数据
                AlarmRecordsData.Headers = new[] { "序号", "类型", "---报警内容---", "报警工站", "报警时间","状态" };

                // 更新CsvTableDataMode
                AlarmRecordsData.Rows = records;

                System.Diagnostics.Debug.WriteLine($"加载了 {records.Count} 条报警记录");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载报警记录时出错: {ex.Message}");
            }
        }
        /// <summary>
        /// 获取本地存储的报警数据
        /// </summary>
        /// <param name="date">哪一天的数据</param>
        /// <returns>读取的数据</returns>
        private async Task<List<string[]>> LocalAlarmData(DateTime date)
        {
            string csvAddress = @".\AlarmLog\" + Device.deviceMessage.DeviecName + @"\" + date.ToString("yyyy_MM_dd") + ".csv";
            var _records = new List<string[]>();

            if (!File.Exists(csvAddress))
            {
                return _records;
            }

            try
            {
                _records = await CsvFileHelper.ReadAllLinesAndSplitAsync(csvAddress, ',', true);
                // 按时间倒序排序
                return _records.OrderByDescending(r => r[4]).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"执行异步读取任务失败: {ex.Message}");
                return new List<string[]>();
            }
        }

        /// <summary>
        /// 启动实时报警监控
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void StartAlarmMonitoring()
        {
            // 实时报警更新
            IDispatcher dispatcher = Application.Current?.Dispatcher ?? throw new InvalidOperationException("No dispatcher available");

            Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine($"实时报警更新：开启");
                while (true)
                {
                    await Task.Delay(200); // 每200毫秒更新一次

                    await dispatcher.DispatchAsync(() =>
                    {
                        UpdateActiveAlarms();
                    });
                }
            });
        }

        /// <summary>
        /// 更新实时报警列表
        /// </summary>
        private void UpdateActiveAlarms()
        {
            try
            {
                // 从PLC读取实时报警数据
                if (!IsDebug)
                {
                    GetPlcActiveAlarms();
                    GetPlcActiveInfos();
                }

                ActiveAlarmCount = ActiveAlarms.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新实时报警时出错: {ex.Message}");
            }
        }
        /// <summary>
        /// 监控PLC报警
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void GetPlcActiveAlarms()
        {
            var systemAlarm = PlcClient.ReadClient.Read(Device.alarmListConfig.SystemAlarm.PlcVarAddress);
            if (systemAlarm is not Int32) return;
            if ((Int32)systemAlarm == _systemAlarm || _systemAlarm == 0) return;
            _systemAlarm = (Int32)systemAlarm;

            List<string> AlarmAddres = new();
            foreach (var item in Device.alarmListConfig.AlarmList)
            {
                AlarmAddres.Add(item.Trigger.PlcVarAddress);
            }

            var lastAlarmData = PlcClient.ReadClient.Read(AlarmAddres);
            if (lastAlarmData is List<bool> currentAlarms)
            {
                // 如果是第一次读取，初始化上次状态列表
                if (_LastAlarmData.Count == 0)
                {
                    _LastAlarmData = currentAlarms.ToList();
                    return;
                }

                // 使用LINQ和并行处理来查找状态变化
                var changes = currentAlarms.Select((state, index) => new { state, index })
                    .AsParallel()
                    .Where(x => x.index >= _LastAlarmData.Count || x.state != _LastAlarmData[x.index])
                    .ToList();

                if (changes.Any())
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var change in changes)
                        {
                            var alarmConfig = Device.alarmListConfig.AlarmList[change.index];

                            if (change.state) // 新报警产生
                            {
                                var newAlarm = new AlarmRecord
                                {
                                    Id = ActiveAlarms.Count + 1,
                                    Type = AlarmType.Alarm,
                                    Content = alarmConfig.Message,
                                    Station = alarmConfig.Station ?? "未知工站",
                                    Time = DateTime.Now,
                                    IsActive = true
                                };
                                ActiveAlarms.Insert(0, newAlarm);
                                WriteAlarmToLocal(newAlarm, true);
                            }
                            else // 报警解除
                            {
                                var alarmToRemove = ActiveAlarms.FirstOrDefault(a =>
                                    a.Content == alarmConfig.Message &&
                                    a.Station == (alarmConfig.Station ?? "未知工站"));

                                if (alarmToRemove != null)
                                {
                                    ActiveAlarms.Remove(alarmToRemove);
                                    WriteAlarmToLocal(alarmToRemove, false);
                                }
                            }
                        }
                    });
                }

                // 更新上次状态记录
                _LastAlarmData = currentAlarms.ToList();
            }
        }
        /// <summary>
        /// 监控PLC提示
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void GetPlcActiveInfos()
        {
            var systemInfo = PlcClient.ReadClient.Read(Device.alarmListConfig.SystemInfo.PlcVarAddress);
            if (systemInfo is not Int32) return;
            if ((Int32)systemInfo == _systemInfo || _systemInfo == 0) return;
            _systemInfo = (Int32)systemInfo;

            List<string> InfoAddres = new();
            foreach (var item in Device.alarmListConfig.InfoList)
            {
                InfoAddres.Add(item.Trigger.PlcVarAddress);
            }

            var lastInfoData = PlcClient.ReadClient.Read(InfoAddres);
            if (lastInfoData is List<bool> currentInfos)
            {
                // 如果是第一次读取，初始化上次状态列表
                if (_LastInfoData.Count == 0)
                {
                    _LastInfoData = currentInfos.ToList();
                    return;
                }

                // 使用LINQ和并行处理来查找状态变化
                var changes = currentInfos.Select((state, index) => new { state, index })
                    .AsParallel()
                    .Where(x => x.index >= _LastInfoData.Count || x.state != _LastInfoData[x.index])
                    .ToList();

                if (changes.Any())
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var change in changes)
                        {
                            var alarmConfig = Device.alarmListConfig.InfoList[change.index];

                            if (change.state) // 新提示产生
                            {
                                var newAlarm = new AlarmRecord
                                {
                                    Id = ActiveAlarms.Count + 1,
                                    Type = AlarmType.Info,
                                    Content = alarmConfig.Message,
                                    Station = alarmConfig.Station ?? "未知工站",
                                    Time = DateTime.Now,
                                    IsActive = true
                                };
                                ActiveAlarms.Insert(0, newAlarm);
                                WriteAlarmToLocal(newAlarm, true);
                            }
                            else // 提示解除
                            {
                                var alarmToRemove = ActiveAlarms.FirstOrDefault(a =>
                                    a.Content == alarmConfig.Message &&
                                    a.Station == (alarmConfig.Station ?? "未知工站"));

                                if (alarmToRemove != null)
                                {
                                    ActiveAlarms.Remove(alarmToRemove);
                                    WriteAlarmToLocal(alarmToRemove, false);
                                }

                            }
                        }
                    });
                }

                // 更新上次状态记录
                _LastInfoData = currentInfos.ToList();
            }
        }
        /// <summary>
        /// 将报警触发写入到本地
        /// </summary>
        /// <param name="alarm"></param>
        private void WriteAlarmToLocal(AlarmRecord alarm, bool Active)
        {
            string CsvAddress = @".\AlarmLog\" + Device.deviceMessage.DeviecName + @"\" + DateTime.Now.ToString("yyyy_MM_dd") + ".csv";
            string AlarmMessage = alarm.Id + "," + alarm.Type.ToString() + "," + alarm.Content + "," + alarm.Station + "," + alarm.Time + "," + Active;
            if (!File.Exists(CsvAddress))
            {
                List<string> headers = new()
            {
                "报警序号",
                "报警类型",
                "报警内容",
                "报警工站",
                "报警时间",
                "触发状态"
            };
                CsvFileHelper.CreateNewCsvFile(CsvAddress, headers);
            }
            CsvFileHelper.AddNewLine(CsvAddress, AlarmMessage);
            alarm.IsActive = false;
            string[] alarmRow = new[]
            {
                alarm.Id.ToString(),
                alarm.Type.ToString(),
                alarm.Content,
                alarm.Station,
                alarm.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                Active.ToString()
            };
            AlarmRecordsData.Rows.Insert(0, alarmRow);

        }
        /// <summary>
        /// 添加测试用实时报警
        /// </summary>
        private void AddTestActiveAlarms()
        {
            // 这里使用模拟数据
            var currentTime = DateTime.Now;

            // 模拟随机添加或移除报警
            if (Random.Shared.Next(2) == 0 && ActiveAlarms.Count < 5)
            {
                var newAlarm = new AlarmRecord
                {
                    Id = Random.Shared.Next(1000),
                    Type = Random.Shared.Next(2) == 0 ? AlarmType.Alarm : AlarmType.Info,
                    Content = $"测试报警 {Random.Shared.Next(100)}",
                    Station = $"工站 {Random.Shared.Next(1, 5)}",
                    Time = currentTime,
                    IsActive = true
                };

                ActiveAlarms.Insert(0, newAlarm);
                WriteAlarmToLocal(newAlarm, true);
            }
            else if (ActiveAlarms.Count > 0)
            {
                WriteAlarmToLocal(ActiveAlarms[ActiveAlarms.Count - 1], false);
                ActiveAlarms.RemoveAt(ActiveAlarms.Count - 1);
            }
        }
        // 用于清理资源的方法
        public void Dispose()
        {
            // 在实际应用中，这里需要清理定时器等资源
        }

        public override void UpdataView()
        {
            throw new NotImplementedException();
        }

        public override void OnViewVisible()
        {
            base.NowView = this.ViewIndex;
        }

        public override void CloseViewVisible()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 报警记录数据模型
    /// </summary>
    public partial class AlarmRecord : ObservableObject
    {
        [ObservableProperty]
        public int id;

        [ObservableProperty]
        public AlarmType type;

        [ObservableProperty]
        public string content = string.Empty;

        [ObservableProperty]
        public string station = string.Empty;

        [ObservableProperty]
        public DateTime time;

        [ObservableProperty]
        public bool isActive;
    }

    /// <summary>
    /// 报警类型枚举
    /// </summary>
    public enum AlarmType
    {
        Alarm,  // 报警
        Info  // 提示
    }

    /// <summary>
    /// 报警过滤类型枚举
    /// </summary>
    public enum AlarmFilterType
    {
        All = 0,    // 全部
        Alarm = 1,  // 报警
        Info = 2  // 提示
    }
}
