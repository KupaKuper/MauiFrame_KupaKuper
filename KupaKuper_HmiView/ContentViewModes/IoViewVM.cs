using KupaKuper_HmiView.Resources;

using KupaKuper_IO.Ethernet;

using KupaKuper_MauiControl.ControlModes;

using System.Collections.ObjectModel;

namespace KupaKuper_HmiView.ContentViewModes
{
    // IO视图的ViewModel类，继承自ObservableObject以支持属性更新通知
    public partial class IoViewVM : BaseViewMode
    {
        #region 私有字段
        // 存储所有当前显示类型（输入或输出）的IO点列表
        private List<IoDataMode> _allIoNames = new();
        
        // 存储输入IO点的列表
        private List<IoDataMode> _inputIoNames = new();
        
        // 存储输出IO点的列表
        private List<IoDataMode> _outputIoNames = new();
        
        // 当前显示的IO点集合
        private ObservableCollection<IoDataMode> _currentIoItems = new();
        
        // 标记当前显示的是否为输入IO（true为输入，false为输出）
        private bool _isShowingInputs = true;
        
        // 添加取消令牌源，用于控制IO监控任务
        private CancellationTokenSource? _monitoringTokenSource;
        
        // 标记当前页面是否处于活动状态
        private bool _isPageActive;
        
        // 添加页面状态控制
        private readonly object _lock = new object();
        #endregion

        #region 公共属性
        // 控制当前显示的是输入还是输出IO
        public bool IsShowingInputs
        {
            get => _isShowingInputs;
            set
            {
                if (_isShowingInputs != value)
                {
                    _isShowingInputs = value;
                    _allIoNames = _isShowingInputs ? _inputIoNames : _outputIoNames;
                    UpdateCurrentIoItems();
                    OnPropertyChanged(nameof(IsShowingInputs));
                    UpdateCommandStates();
                }
            }
        }

        // 当前显示的IO点集合
        public ObservableCollection<IoDataMode> CurrentIoItems
        {
            get
            {
                lock (_lock)
                {
                    return _currentIoItems;
                }
            }
            private set
            {
                lock (_lock)
                {
                    if (_currentIoItems != value)
                    {
                        _currentIoItems = value;
                        OnPropertyChanged();
                    }
                }
            }
        }
        #endregion

        #region 命令
        // 切换到输入IO的命令
        public Command ShowInputsCommand { get; private set; }
        
        // 切换到输出IO的命令
        public Command ShowOutputsCommand { get; private set; }
        public override uint ViewIndex { get; set; }
        public override string ViewName { get => AppResources.ResourceManager?.GetString("IO监控") ?? "IO监控"; set => throw new NotImplementedException(); }

        #endregion

        #region 构造函数
        public IoViewVM()
        {
            // 初始化所有集合
            _currentIoItems = new();

            // 加载IO配置
            if (IsDebug)
            {
                InitializeTestData();
            }
            else
            {
                LoadIOConfig();
            }

            // 默认显示输入IO列表
            _allIoNames = _inputIoNames;

            // 初始化输入/输出切换命令
            ShowInputsCommand = new Command(
                execute: () => IsShowingInputs = true,
                canExecute: () => !IsShowingInputs  // 当前不是输入IO时才可执行
            );

            ShowOutputsCommand = new Command(
                execute: () => IsShowingInputs = false,
                canExecute: () => IsShowingInputs   // 当前是输入IO时才可执行
            );

            // 更新当前显示的IO项
            UpdateCurrentIoItems();
        }

        // 启动实时IO监控
        private void StartIOMonitoring()
        {
            // 如果已经有监控任务在运行，先停止它
            StopIOMonitoring();
            
            // 创建新的取消令牌源
            _monitoringTokenSource = new CancellationTokenSource();
            var token = _monitoringTokenSource.Token;
            
            // 获取调度器用于UI更新
            IDispatcher dispatcher = Application.Current?.Dispatcher ?? 
                throw new InvalidOperationException("No dispatcher available");

            // 启动监控任务
            Task.Run(async () =>
            {
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
                                UpdateIoValue();
                            });
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // 正常的取消操作，不需要特殊处理
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"实时IO更新出错：{ex.Message}");
                }
            }, token);
        }

        /// <summary>
        /// 停止IO监控
        /// 取消正在运行的监控任务
        /// </summary>
        private void StopIOMonitoring()
        {
            if (_monitoringTokenSource != null)
            {
                _monitoringTokenSource.Cancel();
                _monitoringTokenSource.Dispose();
                _monitoringTokenSource = null;
            }
        }

        /// <summary>
        /// 读取IO状态更新到页面
        /// </summary>
        private void UpdateIoValue()
        {
            if (IsDebug) return;
            try
            {
                // 仅当页面激活时执行更新
                if (!_isPageActive) return;

                // 获取当前页面IO的快照
                List<IoDataMode> currentIos;
                lock (_lock)
                {
                    if (_currentIoItems == null) return;
                    currentIos = _currentIoItems.ToList();
                }

                // 使用HashSet存储需要更新的变量地址，避免重复
                var varsToRead = new HashSet<string>();
                foreach (var item in currentIos)
                {
                    if (item?.IoVar?.PlcVarAddress != null)
                    {
                        varsToRead.Add(item.IoVar.PlcVarAddress);
                    }
                }

                // 批量读取所有变量
                if (PlcClient.ReadClient.Read(varsToRead.ToList()) is List<bool> values && 
                    values.Count == varsToRead.Count)
                {
                    lock (_lock)
                    {
                        // 确保集合引用没有改变
                        if (!ReferenceEquals(_currentIoItems.ToList(), currentIos)) return;

                        // 使用字典优化查找
                        var addressValueMap = new Dictionary<string, bool>();
                        int index = 0;
                        foreach (var address in varsToRead)
                        {
                            addressValueMap[address] = values[index++];
                        }

                        // 更新每个IO的状态
                        foreach (var item in CurrentIoItems)
                        {
                            if (item?.IoVar?.PlcVarAddress != null && 
                                addressValueMap.TryGetValue(item.IoVar.PlcVarAddress, out bool value))
                            {
                                if (item.IoValue != value)
                                {
                                    item.IoValue = value;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"实时IO更新异常: {ex.Message}");
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 页面显示时调用此方法
        /// </summary>
        public void OnPageAppearing()
        {
            _isPageActive = true;
            StartIOMonitoring();
        }

        /// <summary>
        /// 页面消失时调用此方法
        /// </summary>
        public void OnPageDisappearing()
        {
            _isPageActive = false;
            StopIOMonitoring();
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 创建测试数据
        /// </summary>
        private void InitializeTestData()
        {
            _inputIoNames = new List<IoDataMode>();
            _outputIoNames = new List<IoDataMode>();

            // 初始化200个输入IO点
            for (int i = 1; i <= 200; i++)
            {
                _inputIoNames.Add(new IoDataMode { IoName = $"输入IO_{i}", IoValue = i % 2 == 0 });
            }

            // 初始化200个输出IO点
            for (int i = 201; i <= 400; i++)
            {
                _outputIoNames.Add(new IoDataMode { IoName = $"输出IO_{i}", IoValue = i % 2 == 0 });
            }
        }
        /// <summary>
        /// 读取本地配置信息
        /// </summary>
        private void LoadIOConfig()
        {
            try
            {
                var IOList = Device?.ioListConfig;
                if (IOList == null || !(IOList.InputIoList.Count>0) || !(IOList.OutputIoList.Count>0))
                {
                    System.Diagnostics.Debug.WriteLine("IO配置为空，使用测试数据");
                    InitializeTestData();
                    return;
                }

                // 使用并行处理来加载IO配置
                _inputIoNames = new List<IoDataMode>(IOList.InputIoList.Count);
                _outputIoNames = new List<IoDataMode>(IOList.OutputIoList.Count);

                // 预分配容量
                for (int i = 0; i < IOList.InputIoList.Count; i++)
                {
                    _inputIoNames.Add(null);
                }
                for (int i = 0; i < IOList.OutputIoList.Count; i++)
                {
                    _outputIoNames.Add(null);
                }

                // 并行处理输入IO
                Parallel.For(0, IOList.InputIoList.Count, i =>
                {
                    var input = IOList.InputIoList[i];
                    _inputIoNames[i] = new IoDataMode
                    {
                        IoName = input.Name,
                        IoVar = input.IoVar
                    };
                });

                // 并行处理输出IO
                Parallel.For(0, IOList.OutputIoList.Count, i =>
                {
                    var output = IOList.OutputIoList[i];
                    _outputIoNames[i] = new IoDataMode
                    {
                        IoName = output.Name,
                        IoVar = output.IoVar
                    };
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadIOConfig 错误: {ex}");
                InitializeTestData();
            }
        }

        // 更新当前显示的IO项
        private void UpdateCurrentIoItems()
        {
            try
            {
                lock (_lock)
                {
                    // 创建新的集合
                    var newItems = new ObservableCollection<IoDataMode>();

                    // 添加所有IO项
                    foreach (var item in _allIoNames)
                    {
                        if (item != null)
                        {
                            newItems.Add(item);
                        }
                    }

                    // 一次性更新集合引用
                    CurrentIoItems = newItems;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateCurrentIoItems 错误: {ex}");
            }
        }

        // 更新命令状态的辅助方法
        private void UpdateCommandStates()
        {
            ShowInputsCommand?.ChangeCanExecute();
            ShowOutputsCommand?.ChangeCanExecute();
        }

        public override void UpdataView()
        {
            if (CurrentIoItems == null) return;
            var random = new Random();
            for (int i = 0; i < 50; i++)
            {
                var c = random.Next(0, CurrentIoItems.Count);
                CurrentIoItems[c].IoValue = !CurrentIoItems[c].IoValue;
            }
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