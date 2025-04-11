using KupaKuper_IO.Ethernet;

namespace KupaKuper_IO.PlcConfig
{
    /// <summary>
    /// 气缸变量名称类，用于存储气缸相关的PLC变量和状态
    /// </summary>
    public class Cylinder
    {
        #region 名称属性

        /// <summary>
        /// 获取或设置气缸名称
        /// </summary>
        public string Name { get; set; } = "**测试使用气缸**";

        /// <summary>
        /// 所属工站名称
        /// </summary>
        public string Station { get; set; } = "";

        /// <summary>
        /// 至原位名称
        /// </summary>
        public string HomeButtonName { get; set; } = "缩回";

        /// <summary>
        /// 至工作位名称
        /// </summary>
        public string WorkButtonName { get; set; } = "伸出";

        #endregion 名称属性

        #region 缩回相关属性

        /// <summary>
        /// 获取或设置缩回控制PLC变量
        /// </summary>
        public PlcVar Home { get; set; } = new() { VarInfo = "缩回控制" };

        /// <summary>
        /// 获取或设置缩回到位信号PLC变量
        /// </summary>
        public PlcVar HomeInput { get; set; } = new() { VarInfo = "缩回到位信号" };

        /// <summary>
        /// 获取或设置缩回完成信号PLC变量
        /// </summary>
        public PlcVar HomeDown { get; set; } = new() { VarInfo = "缩回完成信号" };

        #endregion 缩回相关属性

        #region 伸出相关属性

        /// <summary>
        /// 获取或设置伸出控制PLC变量
        /// </summary>
        public PlcVar Work { get; set; } = new() { VarInfo = "伸出控制" };

        /// <summary>
        /// 获取或设置伸出到位信号PLC变量
        /// </summary>
        public PlcVar WorkInput { get; set; } = new() { VarInfo = "伸出到位信号" };

        /// <summary>
        /// 获取或设置伸出完成信号PLC变量
        /// </summary>
        public PlcVar WorkDown { get; set; } = new() { VarInfo = "伸出完成信号" };

        #endregion 伸出相关属性

        #region 锁定相关属性

        /// <summary>
        /// 获取或设置缩回锁定PLC变量
        /// </summary>
        public PlcVar HomeLock { get; set; } = new() { VarInfo = "缩回锁定" };

        /// <summary>
        /// 获取或设置伸出锁定PLC变量
        /// </summary>
        public PlcVar WorkLock { get; set; } = new() { VarInfo = "伸出锁定" };

        /// <summary>
        /// 获取或设置整体锁定PLC变量
        /// </summary>
        public PlcVar Lock { get; set; } = new() { VarInfo = "运动锁定" };

        #endregion 锁定相关属性

        #region 错误相关属性

        /// <summary>
        /// 获取或设置错误状态PLC变量
        /// </summary>
        public PlcVar Error { get; set; } = new() { VarInfo = "错误状态" };

        #endregion 错误相关属性
    }
}