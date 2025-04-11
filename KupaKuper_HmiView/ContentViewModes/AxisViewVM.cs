using KupaKuper_HmiView.Resources;

using KupaKuper_IO.Ethernet;

using KupaKuper_MauiControl.ControlModes;

using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace KupaKuper_HmiView.ContentViewModes
{
    // 轴控制视图的ViewModel类，实现属性变更通知接口
    public class AxisViewVM : BaseViewMode
    {
        // 标记当前页面是否处于活动状态
        private bool _isPageActive;
        // 添加取消令牌源，用于控制IO监控任务
        private CancellationTokenSource? _monitoringTokenSource;
        // 添加一个用于同步的对象
        private readonly object _axisLock = new object();

        // 构造函数
        public AxisViewVM()
        {
            // 根据运行模式选择数据源
            if (IsDebug)
            {
                InitializeTestData();
            }
            else
            {
                // 从设备配置中加载轴数据
                foreach (var axis in Device.axisListConfig.AxisList)
                {
                    var axisPosition = new List<AxisPositionMode>();
                    foreach (var item in axis.ListPosition)
                    {
                        axisPosition.Add(new()
                        {
                            AxisPosition = item
                        });
                    }
                    AxisVars.TryAdd(axis.AxisControl.Name, new()
                    {
                        Axis = axis.AxisControl,
                        ListPosition = axisPosition
                    });
                }
            }

            // 初始化轴名称列表
            int n = 0;
            foreach (var item in AxisVars.Values)
            {
                n++;
                AxisNames.Add(item.Axis.Name);
            }
        }

        /// <summary>
        /// 生成测试用的轴数据
        /// </summary>
        private void InitializeTestData()
        {
            // 创建10个测试轴
            for (int i = 1; i <= 10; i++)
            {
                var axisVar = new AxisDataMode
                {
                    Axis = new() { Name = $"测试轴_{i}" },
                    ListPosition = new List<AxisPositionMode>()
                };

                // 为每个轴添加5个测试点位
                for (int j = 1; j <= 5; j++)
                {
                    axisVar.ListPosition.Add(new AxisPositionMode
                    {
                        AxisPosition = new() { PositionNo = j },
                    });
                }
                AxisVars.TryAdd(axisVar.Axis.Name, axisVar);
            }
        }
        // 属性变更事件
        public new event PropertyChangedEventHandler? PropertyChanged;

        // 触发属性变更通知的方法
        public new void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// 轴界面绑定的轴数据
        /// </summary>
        public AxisDataMode AxisVar
        {
            get
            {
                lock (_axisLock)
                {
                    return _axisVar;
                }
            }
            set
            {
                lock (_axisLock)
                {
                    if (_axisVar != value)
                    {
                        _axisVar = value;
                        // 更新轴位置字典
                        AxisPosition = value.ListPosition.ToDictionary(
                            item => item.AxisPosition.PositionNo.ToString(),
                            itemValue => itemValue
                        );
                        OnPropertyChanged(nameof(AxisVar));
                    }
                }
            }
        }

        // 默认轴数据
        private AxisDataMode _axisVar = new() { Axis = new() { Name = "测试使用轴_Axis" } };

        /// <summary>
        /// 所有轴的名称列表
        /// </summary>
        public List<string> AxisNames { get; set; } = [];

        /// <summary>
        /// 所有轴的信息字典，键为轴名称
        /// </summary>
        public Dictionary<string, AxisDataMode> AxisVars { get; set; } = [];

        /// <summary>
        /// 当前轴的所有点位信息字典，键为点位编号
        /// </summary>
        public Dictionary<string, AxisPositionMode> AxisPosition = [];

        /// <summary>
        /// 轴已选择的点位值
        /// </summary>
        public string? AbsPosition
        {
            get => _absPosition;
            set
            {
                if (_absPosition != value)
                {
                    _absPosition = value;
                    OnPropertyChanged();
                }
            }
        }

        public override uint ViewIndex { get; set; }
        public override string ViewName { get => AppResources.ResourceManager?.GetString("轴控界面") ?? "轴控界面"; set => throw new NotImplementedException(); }

        // 已选择点位值的私有字段
        private string? _absPosition = "0000.000";

        // 启动实时Axis监控
        private void StartAxisMonitoring()
        {
            // 如果已经有监控任务在运行，先停止它
            StopAxisMonitoring();

            // 创建新的取消令牌源
            _monitoringTokenSource = new CancellationTokenSource();
            var token = _monitoringTokenSource.Token;

            // 获取调度器用于UI更新
            IDispatcher dispatcher = Application.Current?.Dispatcher ??
                throw new InvalidOperationException("No dispatcher available");

            // 启动监控任务
            Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine("实时Axis更新：开启");
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        // 检查页面是否活动
                        if (!_isPageActive)
                        {
                            await Task.Delay(500, token); // 页面非活动时降低检查频率
                            continue;
                        }

                        await Task.Delay(200, token); // 正常更新频率

                        // 确保页面仍然活动时才进行更新
                        if (_isPageActive)
                        {
                            await dispatcher.DispatchAsync(() =>
                            {
                                UpdateAxisValue();
                            });
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // 正常的取消操作，不需要特殊处理
                    System.Diagnostics.Debug.WriteLine("实时Axis更新：已取消");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"实时Axis更新出错：{ex.Message}");
                }
            }, token);
        }

        /// <summary>
        /// 停止Axis监控
        /// 取消正在运行的监控任务
        /// </summary>
        private void StopAxisMonitoring()
        {
            if (_monitoringTokenSource != null)
            {
                _monitoringTokenSource.Cancel();
                _monitoringTokenSource.Dispose();
                _monitoringTokenSource = null;
                System.Diagnostics.Debug.WriteLine("实时Axis更新：停止");
            }
        }

        /// <summary>
        /// 读取Axis状态更新到页面
        /// </summary>
        private void UpdateAxisValue()
        {
            if (IsDebug) return;
            // 创建临时变量存储当前轴数据，避免长时间持有锁
            AxisDataMode? currentAxis;

            // 首先获取当前轴的快照
            lock (_axisLock)
            {
                // 仅当页面激活且有轴数据时执行更新
                if (!_isPageActive || AxisVar?.Axis == null) return;
                currentAxis = AxisVar;
            }

            try
            {
                // 批量读取所有变量
                var _currentPosition = PlcClient.ReadClient.Read(currentAxis.Axis.CurrentPosition.PlcVarAddress);
                var _power = PlcClient.ReadClient.Read(currentAxis.Axis.Power.PlcVarAddress);
                var _busy = PlcClient.ReadClient.Read(currentAxis.Axis.Busy.PlcVarAddress);
                var _memoryPosition = PlcClient.ReadClient.Read(currentAxis.Axis.MemoryPosition.PlcVarAddress);
                var _error = PlcClient.ReadClient.Read(currentAxis.Axis.Error.PlcVarAddress);
                var _absNumber = PlcClient.ReadClient.Read(currentAxis.Axis.AbsNumber.PlcVarAddress);
                var _jogVelocity = PlcClient.ReadClient.Read(currentAxis.Axis.JogVelocity.PlcVarAddress);
                var _posLimit = PlcClient.ReadClient.Read(currentAxis.Axis.PosLimit.PlcVarAddress);
                var _negLimit = PlcClient.ReadClient.Read(currentAxis.Axis.NegLimit.PlcVarAddress);
                var _origin = PlcClient.ReadClient.Read(currentAxis.Axis.Origin.PlcVarAddress);
                var _homeDown = PlcClient.ReadClient.Read(currentAxis.Axis.HomeDown.PlcVarAddress);
                var _movAbsDone = PlcClient.ReadClient.Read(currentAxis.Axis.MovAbsDone.PlcVarAddress);
                var _movRelDone = PlcClient.ReadClient.Read(currentAxis.Axis.MovRelDone.PlcVarAddress);

                // 再次锁定以更新值
                lock (_axisLock)
                {
                    // 确保轴没有被切换
                    if (currentAxis != AxisVar) return;

                    // 更新位置值（字符串类型）
                    if (_currentPosition.ToString() != currentAxis.CurrentPositionValue) currentAxis.CurrentPositionValue = _currentPosition.ToString() ?? "0000.000";

                    // 更新轴使能类型状态
                    if (_power is bool powerVal && powerVal != currentAxis.PowerValue)
                        currentAxis.PowerValue = powerVal;
                    // 更新轴忙碌类型状态
                    if (_busy is bool busyVal && busyVal != currentAxis.BusyValue)
                        currentAxis.BusyValue = busyVal;
                    // 更新记忆位置（字符串类型）
                    if (_memoryPosition.ToString() != currentAxis.MemoryPositionValue) currentAxis.MemoryPositionValue = _memoryPosition.ToString() ?? "0000.000";
                    // 更新轴报错类型状态
                    if (_error is bool errorVal && errorVal != currentAxis.ErrorValue)
                        currentAxis.ErrorValue = errorVal;
                    // 更新定位编号（整数类型）
                    if ((int)_absNumber != currentAxis.AbsNumberValue)
                        currentAxis.AbsNumberValue = (int)_absNumber;
                    // 更新点动速度（字符串类型）
                    if (_jogVelocity.ToString() != currentAxis.JogVelocityValue) currentAxis.JogVelocityValue = _jogVelocity.ToString() ?? "000.00";
                    // 更新轴正极限类型状态
                    if (_posLimit is bool posLimitVal && posLimitVal != currentAxis.PosLimitValue)
                        currentAxis.PosLimitValue = posLimitVal;
                    //更新轴负极限类型状态
                    if (_negLimit is bool negLimitVal && negLimitVal != currentAxis.NegLimitValue)
                        currentAxis.NegLimitValue = negLimitVal;
                    //更新轴原点类型状态
                    if (_origin is bool originVal && originVal != currentAxis.OriginValue)
                        currentAxis.OriginValue = originVal;
                    //更新轴回原点完成类型状态
                    if (_homeDown is bool homeDoneVal && homeDoneVal != currentAxis.HomeDoneValue)
                        currentAxis.HomeDoneValue = homeDoneVal;
                    //更新轴绝对定位完成类型状态
                    if (_movAbsDone is bool movAbsDoneVal && movAbsDoneVal != currentAxis.MovAbsDoneValue)
                        currentAxis.MovAbsDoneValue = movAbsDoneVal;
                    //更新轴相对定位完成类型状态
                    if (_movRelDone is bool movRelDoneVal && movRelDoneVal != currentAxis.MovRelDoneValue)
                        currentAxis.MovRelDoneValue = movRelDoneVal;
                }

                // 处理点位数据
                if (currentAxis.ListPosition?.Count != 0)
                {
                    var positionVars = new List<string>();
                    var velocityVars = new List<string>();

                    // 收集所有点位的位置和速度变量地址
                    foreach (var position in currentAxis.ListPosition)
                    {
                        positionVars.Add(position.AxisPosition.PositionVar.PlcVarAddress);
                        velocityVars.Add(position.AxisPosition.VelocityVar.PlcVarAddress);
                    }

                    // 批量读取点位数据
                    if (positionVars.Any())
                    {
                        if (PlcClient.ReadClient.Read(positionVars) is List<object> positionValues && positionValues?.Count == positionVars.Count)
                        {
                            lock (_axisLock)
                            {
                                // 再次确认轴没有被切换
                                if (currentAxis != AxisVar) return;

                                for (int i = 0; i < positionValues.Count; i++)
                                {
                                    currentAxis.ListPosition[i].PositionVarValue = positionValues[i].ToString() ?? "0000.000";
                                }
                            }
                        }
                    }

                    // 批量读取速度数据
                    if (velocityVars.Any())
                    {
                        if (PlcClient.ReadClient.Read(velocityVars) is List<object> velocityValues && velocityValues?.Count == velocityVars.Count)
                        {
                            lock (_axisLock)
                            {
                                // 再次确认轴没有被切换
                                if (currentAxis != AxisVar) return;

                                for (int i = 0; i < velocityValues.Count; i++)
                                {
                                    currentAxis.ListPosition[i].VelocityVarValue = velocityValues[i].ToString() ?? "000.00";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateAxisValue 错误: {ex.Message}");
            }
        }

        #region 公共方法
        /// <summary>
        /// 页面显示时调用此方法
        /// </summary>
        public void OnPageAppearing()
        {
            _isPageActive = true;
            StartAxisMonitoring();
        }

        /// <summary>
        /// 页面消失时调用此方法
        /// </summary>
        public void OnPageDisappearing()
        {
            _isPageActive = false;
            StopAxisMonitoring();
        }

        public override void UpdataView()
        {
            bool[] AxisDataMode = new[]
            {
                AxisVar.BusyValue,
                AxisVar.ErrorValue,
                AxisVar.HomeDoneValue,
                AxisVar.MovAbsDoneValue,
                AxisVar.MovRelDoneValue,
                AxisVar.OriginValue,
                AxisVar.PosLimitValue,
                AxisVar.NegLimitValue,
                AxisVar.PowerValue
            };
            Random random = new();
            for (int i = 0; i < 4; i++)
            {
                var n = random.Next(0, AxisDataMode.Length);
                AxisDataMode[n] = !AxisDataMode[n];
            }
            AxisVar.BusyValue = AxisDataMode[0];
            AxisVar.ErrorValue = AxisDataMode[1];
            AxisVar.HomeDoneValue = AxisDataMode[2];
            AxisVar.MovAbsDoneValue = AxisDataMode[3];
            AxisVar.MovRelDoneValue = AxisDataMode[4];
            AxisVar.OriginValue = AxisDataMode[5];
            AxisVar.PosLimitValue = AxisDataMode[6];
            AxisVar.NegLimitValue = AxisDataMode[7];
            AxisVar.PowerValue = AxisDataMode[8];
        }

        public override void OnViewVisible()
        {
            base.NowView = this.ViewIndex;
            OnPageAppearing();
        }

        public override void CloseViewVisible()
        {
            OnPageDisappearing();
        }
        #endregion
    }
}