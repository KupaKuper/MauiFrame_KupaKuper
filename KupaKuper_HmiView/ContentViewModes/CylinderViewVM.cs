using KupaKuper_HmiView.Resources;

using KupaKuper_IO.Ethernet;

using KupaKuper_MauiControl.ControlModes;

using System.Collections.ObjectModel;
using System.Windows.Input;

namespace KupaKuper_HmiView.ContentViewModes
{
    // 气缸视图的ViewModel类，继承自BindableObject以支持属性绑定
    public partial class CylinderViewVM : BaseViewMode
    {
        // 线程同步锁对象
        private readonly object _lock = new object();

        // 存储气缸组信息的字典，key为组名，value为该组下的气缸列表
        private Dictionary<string, List<CylinderDataMode>> _cylinderGroups;

        // 存储所有组名的可观察集合
        private ObservableCollection<string> _groupNames;

        // 组名列表属性，用于UI绑定
        public ObservableCollection<string> GroupNames
        {
            get => _groupNames;
            private set
            {
                _groupNames = value;
                OnPropertyChanged();
            }
        }

        // 当前选中的组别key
        private string _currentGroupKey;
        public string CurrentGroupKey
        {
            get => _currentGroupKey;
            set
            {
                if (_currentGroupKey != value)
                {
                    _currentGroupKey = value;
                    OnPropertyChanged();
                    // 切换组别时执行组别切换逻辑
                    ExecuteSwitchGroup(value);
                }
            }
        }

        // 标记当前页面是否处于活动状态
        private bool _isPageActive;
        // 添加取消令牌源，用于控制IO监控任务
        private CancellationTokenSource? _monitoringTokenSource;

        /// <summary>
        /// 添加气缸组到集合中
        /// </summary>
        /// <param name="groupKey">组别名称</param>
        /// <param name="cylinders">该组下的气缸列表</param>
        private void AddCylinderGroup(string groupKey, List<CylinderDataMode> cylinders)
        {
            try
            {
                if (!_cylinderGroups.ContainsKey(groupKey))
                {
                    _cylinderGroups[groupKey] = cylinders;
                    GroupNames.Add(groupKey);

                    // 如果是第一个添加的组，设置为当前组
                    if (string.IsNullOrEmpty(CurrentGroupKey))
                    {
                        CurrentGroupKey = groupKey;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddCylinderGroup 错误: {ex}");
            }
        }

        // 当前页面显示的气缸列表
        private ObservableCollection<CylinderDataMode> _currentPageCylinderNames;
        public ObservableCollection<CylinderDataMode> CurrentPageCylinderNames
        {
            get
            {
                lock (_lock)
                {
                    return _currentPageCylinderNames;
                }
            }
            private set
            {
                lock (_lock)
                {
                    if (_currentPageCylinderNames != value)
                    {
                        _currentPageCylinderNames = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        // 搜索文本
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        // 加载状态标志
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        // 页码显示文本
        private string _pageDisplay = "1/1";
        public string PageDisplay
        {
            get => _pageDisplay;
            set
            {
                _pageDisplay = value;
                OnPropertyChanged();
            }
        }

        // 所有气缸名称列表
        public List<CylinderDataMode> _allIoNames;

        // 切换组别的命令
        public ICommand SwitchGroupCommand { get; private set; }

        // 当前选中的组别key
        private string _selectedGroupKey;
        public string SelectedGroupKey
        {
            get => _selectedGroupKey;
            set
            {
                if (_selectedGroupKey != value)
                {
                    _selectedGroupKey = value;
                    OnPropertyChanged();
                }
            }
        }

        public override uint ViewIndex { get; set; }
        public override string ViewName { get => AppResources.ResourceManager?.GetString("气缸控制") ?? "气缸控制"; set => throw new NotImplementedException(); }

        /// <summary>
        /// 从MainPage加载气缸配置数据
        /// </summary>
        private void LoadCylinderConfig()
        {
            try
            {
                _cylinderGroups = new Dictionary<string, List<CylinderDataMode>>();
                GroupNames = new ObservableCollection<string>();

                var cylinderConfigList = Device?.cylinderListConfig.CylinderList;
                var cylinderList = new List<CylinderDataMode>();
                if (cylinderConfigList == null || !cylinderConfigList.Any())
                {
                    System.Diagnostics.Debug.WriteLine("气缸配置为空，使用测试数据");
                    InitializeTestData();
                    return;
                }
                else
                {
                    cylinderConfigList.ForEach(cylinder =>
                    {
                        cylinderList.Add(new()
                        {
                            Cylinder = cylinder
                        });
                    });
                }

                // 按组别分类气缸
                var groupedCylinders = cylinderList
                    .GroupBy(c => c.Cylinder.Station ?? "默认组")
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(c => new CylinderDataMode { Cylinder = new() { Name = c.Cylinder.Name } }).ToList()
                    );

                // 添加到气缸组
                foreach (var group in groupedCylinders)
                {
                    AddCylinderGroup(group.Key, group.Value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadCylinderConfig 错误: {ex}");
                InitializeTestData(); // 发生错误时使用测试数据
            }
        }

        // 构造函数
        public CylinderViewVM()
        {
            try
            {
                // 初始化基本集合
                _cylinderGroups = new Dictionary<string, List<CylinderDataMode>>();
                GroupNames = new ObservableCollection<string>();
                _allIoNames = new List<CylinderDataMode>();
                CurrentPageCylinderNames = new ObservableCollection<CylinderDataMode>();

                // 初始化命令
                InitializeCommands();

                // 根据调试模式选择数据源
                if (IsDebug)
                {
                    InitializeTestData();
                }
                else
                {
                    LoadCylinderConfig();
                }

                // 如果有组别数据，选择第一个组
                if (GroupNames.Any())
                {
                    CurrentGroupKey = GroupNames[0];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CylinderViewVM 构造函数错误: {ex}");
            }
        }

        // 启动实时气缸监控
        private void StartCylinderMonitoring()
        {
            // 如果已经有监控任务在运行，先停止它
            StopCylinderMonitoring();

            // 创建新的取消令牌源
            _monitoringTokenSource = new CancellationTokenSource();
            var token = _monitoringTokenSource.Token;

            // 获取调度器用于UI更新
            IDispatcher dispatcher = Application.Current?.Dispatcher ??
                throw new InvalidOperationException("No dispatcher available");

            // 启动监控任务
            Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine("实时气缸更新：开启");
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
                                UpdateCylinderValue();
                            });
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // 正常的取消操作，不需要特殊处理
                    System.Diagnostics.Debug.WriteLine("实时气缸更新：已取消");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"实时气缸更新出错：{ex.Message}");
                }
            }, token);
        }

        /// <summary>
        /// 停止气缸监控
        /// 取消正在运行的监控任务
        /// </summary>
        private void StopCylinderMonitoring()
        {
            if (_monitoringTokenSource != null)
            {
                _monitoringTokenSource.Cancel();
                _monitoringTokenSource.Dispose();
                _monitoringTokenSource = null;
                System.Diagnostics.Debug.WriteLine("实时气缸更新：停止");
            }
        }

        /// <summary>
        /// 读取气缸状态更新到页面
        /// </summary>
        private void UpdateCylinderValue()
        {
            if (IsDebug) return;
            try
            {
                // 仅当页面激活时执行更新
                if (!_isPageActive) return;

                // 获取当前页面气缸的快照
                List<CylinderDataMode> currentPageCylinders;
                lock (_lock)
                {
                    if (_currentPageCylinderNames == null) return;
                    currentPageCylinders = _currentPageCylinderNames.ToList();
                }

                // 收集所有需要读取的变量地址
                var varsToRead = new List<string>();
                foreach (var item in currentPageCylinders)
                {
                    varsToRead.Add(item.Cylinder.Work.PlcVarAddress);
                    varsToRead.Add(item.Cylinder.WorkDown.PlcVarAddress);
                    varsToRead.Add(item.Cylinder.WorkInput.PlcVarAddress);
                    varsToRead.Add(item.Cylinder.WorkLock.PlcVarAddress);
                    varsToRead.Add(item.Cylinder.Home.PlcVarAddress);
                    varsToRead.Add(item.Cylinder.HomeDown.PlcVarAddress);
                    varsToRead.Add(item.Cylinder.HomeInput.PlcVarAddress);
                    varsToRead.Add(item.Cylinder.HomeLock.PlcVarAddress);
                    varsToRead.Add(item.Cylinder.Lock.PlcVarAddress);
                }

                // 批量读取所有变量
                if (PlcClient.ReadClient.Read(varsToRead) is List<bool> values && 
                    values.Count == varsToRead.Count)
                {
                    lock (_lock)
                    {
                        // 确保集合引用没有改变
                        if (!ReferenceEquals(_currentPageCylinderNames.ToList(), currentPageCylinders)) return;

                        // 更新每个气缸的状态
                        for (int i = 0; i < currentPageCylinders.Count; i++)
                        {
                            var item = currentPageCylinders[i];
                            var cylinder = CurrentPageCylinderNames[i];
                            int baseIndex = i * 9; // 每个气缸9个状态值

                            // 只有当值变化时才更新，减少UI刷新
                            if (item.WorkValue != values[baseIndex])
                                cylinder.WorkValue = values[baseIndex];
                            if (item.WorkDownValue != values[baseIndex + 1])
                                cylinder.WorkDownValue = values[baseIndex + 1];
                            if (item.WorkInputValue != values[baseIndex + 2])
                                cylinder.WorkInputValue = values[baseIndex + 2];
                            if (item.WorkLockValue != values[baseIndex + 3])
                                cylinder.WorkLockValue = values[baseIndex + 3];
                            if (item.HomeValue != values[baseIndex + 4])
                                cylinder.HomeValue = values[baseIndex + 4];
                            if (item.HomeDownValue != values[baseIndex + 5])
                                cylinder.HomeDownValue = values[baseIndex + 5];
                            if (item.HomeInputValue != values[baseIndex + 6])
                                cylinder.HomeInputValue = values[baseIndex + 6];
                            if (item.HomeLockValue != values[baseIndex + 7])
                                cylinder.HomeLockValue = values[baseIndex + 7];
                            if (item.LockValue != values[baseIndex + 8])
                                cylinder.LockValue = values[baseIndex + 8];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"实时更新异常: {ex.Message}");
            }
        }

        #region 公共方法
        /// <summary>
        /// 页面显示时调用此方法
        /// </summary>
        public void OnPageAppearing()
        {
            _isPageActive = true;
            StartCylinderMonitoring();
        }

        /// <summary>
        /// 页面消失时调用此方法
        /// </summary>
        public void OnPageDisappearing()
        {
            _isPageActive = false;
            StopCylinderMonitoring();
        }
        #endregion

        #region 私有方法
        private void InitializeTestData()
        {
            try
            {
                _cylinderGroups = new Dictionary<string, List<CylinderDataMode>>();
                GroupNames = new ObservableCollection<string>();

                // 添加测试组
                AddCylinderGroup("气缸组A", CreateTestCylinders("A组", 100));
                AddCylinderGroup("气缸组B", CreateTestCylinders("B组", 100));
                AddCylinderGroup("气缸组C", CreateTestCylinders("C组", 100));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeTestData 错误: {ex}");
            }
        }
        /// <summary>
        /// 获取测试用气缸组
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private List<CylinderDataMode> CreateTestCylinders(string prefix, int count)
        {
            var cylinders = new List<CylinderDataMode>();
            for (int i = 0; i < count; i++)
            {
                cylinders.Add(new CylinderDataMode { Cylinder = new() { Name = $"{prefix}气缸_{i}" },ErrorValue=true,LockValue=true });
            }
            return cylinders;
        }

        /// <summary>
        /// 初始化气缸组别选择
        /// </summary>
        private void InitializeCommands()
        {
            try
            {
                // 初始化组切换命令
                SwitchGroupCommand = new Command<string>(
                    execute: (groupKey) =>
                    {
                        if (!string.IsNullOrEmpty(groupKey))
                        {
                            ExecuteSwitchGroup(groupKey);
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeCommands 错误: {ex}");
            }
        }
        /// <summary>
        /// 切换到对应组别
        /// </summary>
        /// <param name="groupKey"></param>
        private void ExecuteSwitchGroup(string groupKey)
        {
            try
            {
                if (_cylinderGroups.ContainsKey(groupKey))
                {
                    SelectedGroupKey = groupKey;
                    
                    // 直接将所有气缸加载到当前页面
                    lock (_lock)
                    {
                        var newItems = new ObservableCollection<CylinderDataMode>();
                        foreach (var item in _cylinderGroups[groupKey])
                        {
                            if (item != null)
                            {
                                newItems.Add(item);
                            }
                        }
                        CurrentPageCylinderNames = newItems;
                        _allIoNames = _cylinderGroups[groupKey];
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"切换到组: {groupKey}, 项目数: {_allIoNames.Count}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExecuteSwitchGroup 错误: {ex}");
            }
        }

        public override void UpdataView()
        {
            if (CurrentPageCylinderNames == null) return;
            var random = new Random();
            for (int i = 0; i < 50; i++)
            {
                var c = random.Next(0, CurrentPageCylinderNames.Count);
                CurrentPageCylinderNames[c].ErrorValue = !CurrentPageCylinderNames[c].ErrorValue;
                CurrentPageCylinderNames[c].LockValue = !CurrentPageCylinderNames[c].LockValue;
                CurrentPageCylinderNames[c].WorkValue = !CurrentPageCylinderNames[c].WorkValue;
                CurrentPageCylinderNames[c].HomeValue = !CurrentPageCylinderNames[c].HomeValue;
                CurrentPageCylinderNames[c].HomeInputValue = !CurrentPageCylinderNames[c].HomeInputValue;
                CurrentPageCylinderNames[c].HomeDownValue = !CurrentPageCylinderNames[c].HomeDownValue;
                CurrentPageCylinderNames[c].WorkInputValue = !CurrentPageCylinderNames[c].WorkInputValue;
                CurrentPageCylinderNames[c].WorkDownValue = !CurrentPageCylinderNames[c].WorkDownValue;
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