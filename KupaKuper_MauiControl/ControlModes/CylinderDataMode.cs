using CommunityToolkit.Mvvm.ComponentModel;

using KupaKuper_IO.PlcConfig;

namespace KupaKuper_MauiControl.ControlModes
{
    /// <summary>
    /// 气缸绑定数据模型
    /// </summary>
    public partial class CylinderDataMode : ObservableObject
    {
        /// <summary>
        /// 气缸变量
        /// </summary>
        [ObservableProperty]
        public Cylinder cylinder = new();
        #region 缩回相关属性
        /// <summary>
        /// 缩回控制的当前值
        /// </summary>
        [ObservableProperty]
        public bool homeValue = false;
        /// <summary>
        /// 缩回到位信号的当前值
        /// </summary>
        [ObservableProperty]
        public bool homeInputValue = false;
        /// <summary>
        /// 缩回动作的当前值
        /// </summary>
        [ObservableProperty]
        public bool homeDownValue = false;
        #endregion

        #region 伸出相关属性
        /// <summary>
        /// 伸出控制的当前值
        /// </summary>
        [ObservableProperty]
        public bool workValue = false;
        /// <summary>
        /// 伸出到位信号的当前值
        /// </summary>
        [ObservableProperty]
        public bool workInputValue = false;
        /// <summary>
        /// 伸出动作的当前值
        /// </summary>
        [ObservableProperty]
        public bool workDownValue = false;
        #endregion

        #region 锁定相关属性
        /// <summary>
        /// 缩回锁定的当前值
        /// </summary>
        [ObservableProperty]
        public bool homeLockValue = false;
        /// <summary>
        /// 伸出锁定的当前值
        /// </summary>
        [ObservableProperty]
        public bool workLockValue = false;
        /// <summary>
        /// 整体锁定的当前值
        /// </summary>
        [ObservableProperty]
        public bool lockValue = false;
        #endregion

        #region 错误相关属性
        /// <summary>
        /// 错误状态的当前值
        /// </summary>
        [ObservableProperty]
        public bool errorValue = false;
        #endregion

    }
}
