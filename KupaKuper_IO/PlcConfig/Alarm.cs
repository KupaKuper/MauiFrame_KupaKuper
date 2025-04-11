using KupaKuper_IO.Ethernet;

namespace KupaKuper_IO.PlcConfig
{
    public class Alarm
    {
        /// <summary>
        /// 报警触发变量
        /// </summary>
        public PlcVar Trigger { get; set; } = new() { VarInfo = "报警的触发变量" };

        /// <summary>
        /// 报警显示信息
        /// </summary>
        public string Message { get; set; } = "报警显示信息";

        /// <summary>
        /// 报警所属工站
        /// </summary>
        public string Station { get; set; } = "Null";
    }
}