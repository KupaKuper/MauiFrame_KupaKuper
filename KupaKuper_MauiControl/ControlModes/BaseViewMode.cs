using CommunityToolkit.Mvvm.ComponentModel;

using KupaKuper_IO.PlcConfig;

using PlcDevice = KupaKuper_IO.Ethernet.PlcClient;

namespace KupaKuper_MauiControl.ControlModes
{
    public abstract partial class BaseViewMode : ObservableObject, IViewVisibleAware
    {
        /// <summary>
        /// 是否调试
        /// </summary>
        public readonly bool IsDebug = true;

        /// <summary>
        /// 设备配置参数
        /// </summary>
        public readonly DeviceConfig Device = PlcDevice.device;

        /// <summary>
        /// 当前显示的页面
        /// </summary>
        public uint NowView
        {
            get => _nowViwe;
            set
            {
                if (value != _nowViwe)
                {
                    LastView = _nowViwe;
                    _nowViwe = value;
                }
            }
        }

        private uint _nowViwe = 1;

        /// <summary>
        /// 上一次显示的页面
        /// </summary>
        public uint LastView { get; private set; } = 0;

        /// <summary>
        /// 页面序号
        /// 1:home
        /// 2:IO
        /// 3:Axis
        /// 4:Cylinder
        /// 5:OtherSetting
        /// 6:VisionImage
        /// 7:Alarm
        /// 8:DailyProduction
        /// 9:HistoryProduction
        /// 10:FaultStatistics
        /// 11:DataRecord
        /// 12:LogIn
        /// 13:SystemSet
        /// </summary>
        public abstract uint ViewIndex { get; set; }

        /// <summary>
        /// 页面的名称,会显示在页面切换框内
        /// </summary>
        public abstract string ViewName { get; set; }

        /// <summary>
        /// 页面调试刷新方法
        /// </summary>
        public abstract void UpdataView();

        /// <summary>
        /// 页面显示时触发的方法
        /// </summary>
        public abstract void OnViewVisible();

        /// <summary>
        /// 页面退出显示时触发的方法
        /// </summary>
        public abstract void CloseViewVisible();
    }
}