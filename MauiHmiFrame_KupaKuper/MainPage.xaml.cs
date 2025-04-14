using KupaKuper_HmiView.ContentViews;
using KupaKuper_HmiView.HelpVoid;

using KupaKuper_IO.Ethernet;

using LocalizationResourceManager.Maui;

using MauiHmiFrame_KupaKuper.Views;

namespace MauiHmiFrame_KupaKuper
{
    /// <summary>
    /// 用户类型
    /// </summary>
    public enum UserType
    {
        Operator,
        Administrator
    }
    /// <summary>
    /// 上层页面名称
    /// </summary>
    public enum TopPagIndx
    {
        HomeView = 1,
        Setting = 2,
        Vision = 3,
        Alarm = 4,
        Data = 5,
        SystemSet = 6,
        Login = 7
    }

    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// 当前顶层页面号
        /// </summary>
        public static TopPagIndx TopPagNowIndx;
        /// <summary>
        /// 调试模式
        /// </summary>
        public static bool DebugMode = true;
        private readonly ILocalizationResourceManager resourceManager;
        /// <summary>
        /// 页面绑定的mode类
        /// </summary>
        private MainPageMode _pageMode = new();
        /// <summary>
        /// 定时读取任务委托
        /// </summary>
        public delegate void CyclicRun();
        /// <summary>
        /// 定时执行触发方法
        /// </summary>
        public static event CyclicRun? CyclincRunning;
        /// <summary>
        /// 记录每个页面集合最后一次显示的页面序号(页面集合序号,页面id号)
        /// </summary>
        private Dictionary<uint, uint> PageLastViewIndex = new();

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
        #region 页面定义合集
        private readonly List<Dictionary<uint, ContentView>> Pages=new();
        private readonly Dictionary<uint, ContentView> HomePage=new();
        private readonly Dictionary<uint, ContentView> SettingPage=new();
        private readonly Dictionary<uint, ContentView> VisionPage=new();
        private readonly Dictionary<uint, ContentView> AlarmPage=new();
        private readonly Dictionary<uint, ContentView> DataPage=new();
        private readonly Dictionary<uint, ContentView> SystemSetPage=new();
        private readonly Dictionary<uint, ContentView> LoginPage=new();
        /// <summary>
        /// 首页
        /// </summary>
        private HomeView homeView;
        /// <summary>
        /// Io监控页
        /// </summary>
        private IoView ioView;
        /// <summary>
        /// 轴控制页
        /// </summary>
        private AxisView axisView;
        /// <summary>
        /// 气缸控制页
        /// </summary>
        private CylinderView cylinderView;
        /// <summary>
        /// 参数设置等其它页
        /// </summary>
        private OtherView otherView;
        /// <summary>
        /// 拍照监控页
        /// </summary>
        private VisionImangView visionImangView;
        /// <summary>
        /// 报警监控页
        /// </summary>
        private AlarmView alarmView;
        /// <summary>
        /// 今日生产数据监控页
        /// </summary>
        private DailyProductionView dailyProductionView;
        /// <summary>
        /// 历史生产数据页
        /// </summary>
        private HistoryProductionView historyProductionView;
        /// <summary>
        /// 设备状态监控数据页
        /// </summary>
        private FaultStatisticsView faultStatisticsView;
        /// <summary>
        /// 产品数据监控页
        /// </summary>
        private DataRecordView dataRecordView;
        /// <summary>
        /// 用户登入页
        /// </summary>
        private LoginView loginView;
        /// <summary>
        /// 软件设置页
        /// </summary>
        private SystemSetView systemSetView;
        #endregion
        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            // 实时报警更新
            IDispatcher dispatcher = Application.Current?.Dispatcher ?? throw new InvalidOperationException("No dispatcher available");

            Task.Run(async () =>
            {
                while (true)
                {
                    Thread.Sleep(1000); // 每1000毫秒更新一次
                    await dispatcher.DispatchAsync(() =>
                    {
                        CyclincRunning?.Invoke();
                    });
                }
            });
        }

        public MainPage(ILocalizationResourceManager resourceManager)
        {
            this.resourceManager = resourceManager;
            InitializeComponent();
            InitializeViews();
            InitVM();
            Initialize();
        }

        private void InitVM()
        {
            BindingContext = _pageMode;
            _pageMode.PageButtonWidth = Math.Min(this.Width / 14, 50);
            _pageMode.LogButtonWidth = Math.Min(this.Width / 14 * 3, 200);
            // 监听SelectedViewIndex的变化
            _pageMode.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_pageMode.SelectedViewIndex))
                {
                    uint viewIndex = PageLastViewIndex.TryGetValue(_pageMode.SelectedViewIndex, out viewIndex) ? viewIndex : 1;
                    ChangeHomePage(_pageMode.SelectedViewIndex, viewIndex);
                }
            };
        }

        /// <summary>
        /// 切换页面合集
        /// </summary>
        /// <param name="selectedViewIndex"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ChangeHomePage(uint selectedViewIndex,uint selectedLableIndex)
        {
            if (!(Pages.Count > 0)) return;
            if (Pages[(int)selectedViewIndex - 1].Count > 0)
            {
                switchViews.SetViewSource(Pages[(int)selectedViewIndex - 1], selectedLableIndex);
                TopPagNowIndx = (TopPagIndx)(int)selectedViewIndex;
            }
        }

        /// <summary>
        /// 当前用户
        /// </summary>
        private static UserType UserName { get; set; } = UserType.Operator;


        private void InitializeViews()
        {
            //提前加载需要的界面
            homeView = new();
            HomePage.Add(1, homeView);
            Pages.Add(HomePage);
            // 后台预加载其他视图
            Task.Run(() =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ioView = new();
                    axisView = new();
                    cylinderView = new();
                    otherView = new();
                    visionImangView = new();
                    alarmView = new();
                    dailyProductionView = new();
                    historyProductionView = new();
                    faultStatisticsView = new();
                    dataRecordView = new();
                    loginView = new();
                    systemSetView = new(resourceManager);
                    SettingPage.Add(2, ioView);
                    SettingPage.Add(3, axisView);
                    SettingPage.Add(4, cylinderView);
                    SettingPage.Add(5, otherView);
                    VisionPage.Add(6, visionImangView);
                    AlarmPage.Add(7, alarmView);
                    DataPage.Add(8, dailyProductionView);
                    DataPage.Add(9, historyProductionView);
                    DataPage.Add(10, faultStatisticsView);
                    DataPage.Add(11, dataRecordView);
                    LoginPage.Add(12, loginView);
                    SystemSetPage.Add(13, systemSetView);
                    Pages.Add(SettingPage);
                    Pages.Add(VisionPage);
                    Pages.Add(AlarmPage);
                    Pages.Add(DataPage);
                    Pages.Add(LoginPage);
                    Pages.Add(SystemSetPage);
                });
            });
            ChangeHomePage(1, 1);
        }

        private void ChangeUser(int user)
        {
            UserName = (UserType)user;
            _pageMode.IsAdmini = UserName == UserType.Administrator;
            System.Diagnostics.Debug.WriteLine($"用户类型已切换至{UserName}");
        }

        // 添加上一页记录列表
        private List<(uint,uint)> LastPage = new();

        // 返回上一页的按钮点击事件
        private void LogeButton_Clicked(object sender, EventArgs e)
        {
            if (LastPage.Count > 1) // 修改为 > 1，确保至少有一个历史记录
            {
                // 移除当前页
                LastPage.RemoveAt(LastPage.Count - 1);
                //获取上一页的组别索引
                uint previousPage = LastPage.LastOrDefault().Item1;
                // 获取上一页的索引
                uint previousView = LastPage.LastOrDefault().Item2;
                // 更新switchViews的绑定页面组
                ChangeHomePage(previousPage, previousView);
                // 移除上一页记录（因为切换时会重新添加）
                LastPage.RemoveAt(LastPage.Count - 1);
            }
        }

        private void StartButton_Clicked(object sender, EventArgs e)
        {
            var var = PlcClient.Device.mainSystem.Start;
            if (var.PlcVarAddress == "") return;
            PlcVarSend.ButtonClicked_SetTrue(var);
        }

        private void StopButton_Clicked(object sender, EventArgs e)
        {
            var var = PlcClient.Device.mainSystem.Pause;
            if (var.PlcVarAddress == "") return;
            PlcVarSend.ButtonClicked_SetTrue(var);
        }

        private void ResetButton_Clicked(object sender, EventArgs e)
        {
            var var = PlcClient.Device.mainSystem.Reset;
            if (var.PlcVarAddress == "") return;
            PlcVarSend.ButtonClicked_SetTrue(var);
        }
        /// <summary>
        /// 页面发生改变时触发
        /// </summary>
        /// <param name="ViewIndex"></param>
        private void SwitchViews_CurrentViewChanged(uint ViewIndex)
        {
            LastPage.Add(((_pageMode.SelectedViewIndex), ViewIndex));
            if (!PageLastViewIndex.TryAdd(_pageMode.SelectedViewIndex, ViewIndex))
            {
                PageLastViewIndex[_pageMode.SelectedViewIndex] = ViewIndex;
            }
        }
    }

}
