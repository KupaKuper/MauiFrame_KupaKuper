using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using KupaKuper_HmiView.Resources;

using KupaKuper_IO.Ethernet;
using KupaKuper_IO.PlcConfig;

using KupaKuper_MauiControl.ControlModes;

using System.Collections.ObjectModel;

namespace KupaKuper_HmiView.ContentViewModes
{
    // 其他参数设置视图的ViewModel类，继承自ObservableObject以支持属性更新通知
    public partial class OtherViewVM : BaseViewMode
    {
        // 参数组名称列表，使用ObservableCollection支持UI自动更新
        [ObservableProperty]
        private ObservableCollection<string> parameterListName = new();

        // 当前显示的参数列表，使用ObservableCollection支持UI自动更新
        [ObservableProperty]
        private ObservableCollection<ParameterData> currentParameters = new();

        /// <summary>
        /// 存放所有变量列表信息的字典
        /// key: 参数组名称
        /// value: 该组下的参数列表
        /// </summary>
        public Dictionary<string, List<ParameterData>> ParameterList { get; set; } = new();
        public override uint ViewIndex { get; set; }
        public override string ViewName { get => AppResources.ResourceManager?.GetString("参数设置") ?? "参数设置"; set => throw new NotImplementedException(); }

        // 当前选中的参数组名称
        private string _currentGroup;
        // 标记当前页面是否处于活动状态
        private bool _isPageActive;
        // 添加取消令牌源，用于控制IO监控任务
        private CancellationTokenSource? _monitoringTokenSource;

        /// <summary>
        /// 构造函数：初始化参数设置视图
        /// </summary>
        public OtherViewVM()
        {
            System.Diagnostics.Debug.WriteLine("OtherViewVM 构造函数开始");
            
            // 根据运行模式选择数据源
            if (IsDebug)
            {
                // 调试模式下使用测试数据
                InitializeTestData();
            }
            else
            {
                // 实际运行时从本地配置加载
                LocalSettingConfig();
            }

            // 初始化选择第一个分组
            if (ParameterListName.Count>0)
            {
                System.Diagnostics.Debug.WriteLine($"找到 {ParameterListName.Count} 个分组");
                SwitchGroup(ParameterListName[0]);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("没有找到任何分组");
            }
        }

        /// <summary>
        /// 切换参数组的命令处理方法
        /// </summary>
        /// <param name="groupName">要切换到的参数组名称</param>
        [RelayCommand]
        private void SwitchGroup(string groupName)
        {
            // 参数验证
            if (string.IsNullOrEmpty(groupName))
                return;

            // 更新当前组名称
            _currentGroup = groupName;
            
            // 尝试获取并显示选中组的参数列表
            if (ParameterList.TryGetValue(groupName, out var parameters))
            {
                // 清空当前显示的参数列表
                CurrentParameters.Clear();
                // 添加新的参数列表
                foreach (var param in parameters)
                {
                    CurrentParameters.Add(param);
                }
            }
            System.Diagnostics.Debug.WriteLine($"切换到分组: {groupName}, 参数数量: {CurrentParameters.Count}");
        }

        /// <summary>
        /// 填充测试数据
        /// </summary>
        private void InitializeTestData()
        {
            // 清除旧数据
            ParameterListName.Clear();
            
            // 添加测试用的参数组名称
            ParameterListName.Add("基本设置");
            ParameterListName.Add("高级设置");

            // 创建基本设置组的参数列表
            var basicParams = new List<ParameterData>();
            for (int i = 1; i <= 5; i++)
            {
                basicParams.Add(new ParameterData 
                {
                    Config = new() 
                    { 
                        Name = $"基本参数 {i}",
                        plcVar = new() { PlcVarMode = VarMode.Bool }
                    }
                });
            }
            ParameterList.Add("基本设置", basicParams);

            // 创建高级设置组的参数列表
            var advancedParams = new List<ParameterData>();
            for (int i = 1; i <= 8; i++)
            {
                var param = new ParameterData
                {
                    Config = new() { Name = $"基本参数 {i}" }
                };

                // 偶数项设置为数值类型，奇数项设置为布尔类型
                if (i % 2 == 0)
                {
                    param.Config.plcVar = new PlcVar { PlcVarMode = VarMode.Float };
                }
                else
                {
                    param.Config.plcVar = new PlcVar { PlcVarMode = VarMode.Bool };
                }

                advancedParams.Add(param);
            }
            ParameterList.Add("高级设置", advancedParams);
        }

        /// <summary>
        /// 从本地读取设置变量配置
        /// </summary>
        private void LocalSettingConfig()
        {
            // 清除旧数据
            ParameterListName.Clear();

            // 添加测试用的参数组名称
            ParameterListName.Add("基本设置");
            ParameterListName.Add("高级设置");

            // 创建基本设置组的参数列表
            var basicParams = new List<ParameterData>();
            for (int i = 1; i <= 5; i++)
            {
                basicParams.Add(new ParameterData
                {
                    Config = new()
                    {
                        Name = $"基本参数 {i}",
                        plcVar = new() { PlcVarMode = VarMode.Bool }
                    }
                });
            }
            ParameterList.Add("基本设置", basicParams);

            // 创建高级设置组的参数列表
            var advancedParams = new List<ParameterData>();
            for (int i = 1; i <= 8; i++)
            {
                var param = new ParameterData
                {
                    Config = new() { Name = $"基本参数 {i}" }
                };

                // 偶数项设置为数值类型，奇数项设置为布尔类型
                if (i % 2 == 0)
                {
                    param.Config.plcVar = new PlcVar { PlcVarMode = VarMode.Float };
                }
                else
                {
                    param.Config.plcVar = new PlcVar { PlcVarMode = VarMode.Bool };
                }

                advancedParams.Add(param);
            }
            ParameterList.Add("高级设置", advancedParams);
        }

        // 添加页面状态控制
        private readonly object _lock = new object();

        /// <summary>
        /// 读取配置变量状态更新到页面
        /// </summary>
        private void UpdateOtherValue()
        {
            if (IsDebug) return;
            try
            {
                // 仅当页面激活时执行更新
                if (!_isPageActive) return;

                // 获取当前参数列表的快照
                List<ParameterData> currentParameters;
                lock (_lock)
                {
                    if (CurrentParameters == null) return;
                    currentParameters = CurrentParameters.ToList();
                }

                // 分别收集布尔类型和数值类型的变量地址
                var boolVars = new List<string>();
                var numericVars = new List<string>();
                var boolIndices = new List<int>();
                var numericIndices = new List<int>();

                for (int i = 0; i < currentParameters.Count; i++)
                {
                    var param = currentParameters[i];
                    if (param?.Config?.plcVar?.PlcVarAddress == null) continue;

                    if (param.IsBooleanType)
                    {
                        boolVars.Add(param.Config.plcVar.PlcVarAddress);
                        boolIndices.Add(i);
                    }
                    else if (param.IsNumericType)
                    {
                        numericVars.Add(param.Config.plcVar.PlcVarAddress);
                        numericIndices.Add(i);
                    }
                }

                // 批量读取布尔类型变量
                if (boolVars.Any())
                {
                    if (PlcClient.ReadClient.Read(boolVars) is List<bool> boolValues && 
                        boolValues.Count == boolVars.Count)
                    {
                        lock (_lock)
                        {
                            // 确保集合引用没有改变
                            if (!ReferenceEquals(CurrentParameters.ToList(), currentParameters)) return;

                            // 更新布尔类型参数的值
                            for (int i = 0; i < boolValues.Count; i++)
                            {
                                var paramIndex = boolIndices[i];
                                var param = currentParameters[paramIndex];
                                if (param.Value != boolValues[i])
                                {
                                    param.Value = boolValues[i];
                                }
                            }
                        }
                    }
                }

                // 批量读取数值类型变量
                if (numericVars.Any())
                {
                    if (PlcClient.ReadClient.Read(numericVars) is List<string> numericValues && 
                        numericValues.Count == numericVars.Count)
                    {
                        lock (_lock)
                        {
                            // 确保集合引用没有改变
                            if (!ReferenceEquals(CurrentParameters.ToList(), currentParameters)) return;

                            // 更新数值类型参数的值
                            for (int i = 0; i < numericValues.Count; i++)
                            {
                                var paramIndex = numericIndices[i];
                                var param = currentParameters[paramIndex];
                                if (param.NumericValue != numericValues[i])
                                {
                                    param.NumericValue = numericValues[i];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"实时参数更新异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 启动实时参数监控
        /// </summary>
        private void StartOtherMonitoring()
        {
            // 如果已经有监控任务在运行，先停止它
            StopOtherMonitoring();

            // 创建新的取消令牌源
            _monitoringTokenSource = new CancellationTokenSource();
            var token = _monitoringTokenSource.Token;

            // 获取调度器用于UI更新
            IDispatcher dispatcher = Application.Current?.Dispatcher ??
                throw new InvalidOperationException("No dispatcher available");

            // 启动监控任务
            Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine("实时参数更新：开启");
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
                                UpdateOtherValue();
                            });
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // 正常的取消操作，不需要特殊处理
                    System.Diagnostics.Debug.WriteLine("实时参数更新：已取消");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"实时参数更新出错：{ex.Message}");
                }
            }, token);
        }

        /// <summary>
        /// 停止参数监控
        /// </summary>
        private void StopOtherMonitoring()
        {
            if (_monitoringTokenSource != null)
            {
                _monitoringTokenSource.Cancel();
                _monitoringTokenSource.Dispose();
                _monitoringTokenSource = null;
                System.Diagnostics.Debug.WriteLine("实时参数更新：停止");
            }
        }

        #region 公共方法
        /// <summary>
        /// 页面显示时调用此方法
        /// </summary>
        public void OnPageAppearing()
        {
            _isPageActive = true;
            StartOtherMonitoring();
        }

        /// <summary>
        /// 页面消失时调用此方法
        /// </summary>
        public void OnPageDisappearing()
        {
            _isPageActive = false;
            StopOtherMonitoring();
        }

        public override void UpdataView()
        {
            throw new NotImplementedException();
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

    /// <summary>
    /// 参数绑定数据模型
    /// </summary>
    public partial class ParameterData:ObservableObject
        {
            /// <summary>
            /// 参数配置信息
            /// </summary>
            [ObservableProperty]
            public Parameter config = new();

            /// <summary>
            /// 布尔类型参数的值
            /// </summary>
            [ObservableProperty]
            public bool value = false;

            /// <summary>
            /// 数值类型参数的值
            /// </summary>
            [ObservableProperty]
            public string numericValue = "0";
        
            /// <summary>
            /// 判断参数是否为布尔类型
            /// </summary>
            public bool IsBooleanType => Config?.plcVar.PlcVarMode == VarMode.Bool;

            /// <summary>
            /// 判断参数是否为数值类型（Float、Int16或Int32）
            /// </summary>
            public bool IsNumericType => Config?.plcVar.PlcVarMode == VarMode.Float || 
                                       Config?.plcVar.PlcVarMode == VarMode.Int16 || 
                                       Config?.plcVar.PlcVarMode == VarMode.Int32;
        }
}
