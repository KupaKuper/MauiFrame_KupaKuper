using CommunityToolkit.Mvvm.ComponentModel;

using KupaKuper_HmiView.ContentViews;
using KupaKuper_HmiView.HelpVoid;

using KupaKuper_IO.Ethernet;

using KupaKuper_MauiControl.Controls;
using PlcClient= KupaKuper_IO.Ethernet.PlcClient;

namespace MauiHmiFrame_KupaKuper
{
    public partial class MainPageMode : ObservableRecipient
    {
        /// <summary>
        /// 当前显示页面集合
        /// </summary>
        [ObservableProperty]
        public Dictionary<uint, ContentView> currentViews;
        /// <summary>
        /// 当前选中的页面合集序号
        /// </summary>
        [ObservableProperty]
        public uint selectedViewIndex = 1;  // 默认显示首页
        /// <summary>
        /// PlC连接状态
        /// </summary>
        [ObservableProperty]
        public bool plcConnect = false;
        /// <summary>
        /// 管理员权限
        /// </summary>
        [ObservableProperty]
        public bool isAdmini = false;
        /// <summary>
        /// 选择页面合集的点击事件
        /// </summary>
        [ObservableProperty]
        public Command<string> setViewCommand;
        /// <summary>
        /// 页面切换按钮的宽度
        /// </summary>
        [ObservableProperty]
        public double pageButtonWidth = 57;
        /// <summary>
        /// 页面切换按钮的宽度
        /// </summary>
        [ObservableProperty]
        public double logButtonWidth = 200;
        private void ConnectEll()
        {
            PlcConnect = PlcClient.Plc_Connect;
        }

        public MainPageMode()
        {
            SetViewCommand = new Command<string>(
            execute: (viewIndex) =>
            {
                if (int.TryParse(viewIndex, out int index))
                {
                    SelectedViewIndex = (uint)index;
                }
            });

            if (PlcClient.TryReadConfig())
            {
                PlcClient.plcModel = PlcClient.Device.deviceMessage.DeviceType;
                if (PlcClient.ReadClient != null) PlcClient.ReadClient.ClientUrl = PlcClient.Device.deviceMessage.DeviceAddress;
                if (PlcClient.WriteClient != null) PlcClient.WriteClient.ClientUrl = PlcClient.Device.deviceMessage.DeviceAddress;
                PlcClient.connectEll += ConnectEll;
            }
            else
            {
                DisplayAlertHelp.TryDisplayAlert("读取出错", "配置文件读取错误,请检查文件是否存在或文件格式是否正确", "关闭");
                return;
            }
            List<string> vars = new();
            foreach (var item in PlcClient.Device.cyclicReadListConfig.CyclicReadList)
            {
                vars.Add(item.PlcVarAddress);
            }
        }
    }
}
