using KupaKuper_IO.Ethernet;

namespace KupaKuper_IO.PlcConfig
{
    public class MainSystem
    {
        /// <summary>
        /// 启动
        /// </summary>
        public PlcVar Start { get; set; } = new() { VarInfo = "设备启动" };

        /// <summary>
        /// 复位
        /// </summary>
        public PlcVar Reset { get; set; } = new() { VarInfo = "设备复位" };

        /// <summary>
        /// 暂停
        /// </summary>
        public PlcVar Pause { get; set; } = new() { VarInfo = "设备暂停" };
    }
}