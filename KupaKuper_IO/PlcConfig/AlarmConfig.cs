using KupaKuper_IO.Ethernet;

namespace KupaKuper_IO.PlcConfig
{
    public class AlarmConfig
    {
        /// <summary>
        /// 系统报警触发变量
        /// </summary>
        public PlcVar SystemAlarm { get; set; } = new() { VarInfo = "系统总报警触发变量" };

        /// <summary>
        /// 报警信息列表
        /// </summary>
        public List<Alarm> AlarmList { get; set; } = new() { new(), new(), new(), new(), new() };

        /// <summary>
        /// 系统提示触发变量
        /// </summary>
        public PlcVar SystemInfo { get; set; } = new() { VarInfo = "系统总提示触发变量" };

        /// <summary>
        /// 提示信息列表
        /// </summary>
        public List<Alarm> InfoList { get; set; } = new() { new(), new(), new(), new(), new() };
    }
}