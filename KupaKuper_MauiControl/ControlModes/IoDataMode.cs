using CommunityToolkit.Mvvm.ComponentModel;
using KupaKuper_IO.Ethernet;

namespace KupaKuper_MauiControl.ControlModes
{
    /// <summary>
    /// Io绑定数据模型
    /// </summary>
    public partial class IoDataMode : ObservableObject
    {
        /// <summary>
        /// Io名称
        /// </summary>
        [ObservableProperty]
        public string ioName = string.Empty;
        /// <summary>
        /// Io当前值
        /// </summary>
        [ObservableProperty]
        public bool ioValue = false;
        /// <summary>
        /// Io点的变量
        /// </summary>
        [ObservableProperty]
        public PlcVar ioVar = new();
    }
}
