namespace KupaKuper_IO.PlcConfig
{
    public class DeviceConfig
    {
        /// <summary>
        /// 设备信息
        /// </summary>
        public DeviceMessage deviceMessage { get; set; } = new();

        /// <summary>
        /// PLC设备系统运行变量
        /// </summary>
        public MainSystem mainSystem { get; set; } = new();

        /// <summary>
        /// Io点位变量
        /// </summary>
        public IoLConfig ioListConfig { get; set; } = new();

        /// <summary>
        /// 轴运行变量
        /// </summary>
        public AxisConfig axisListConfig { get; set; } = new();

        /// <summary>
        /// 气缸运行变量
        /// </summary>
        public CylinderConfig cylinderListConfig { get; set; } = new();

        /// <summary>
        /// 报警读取变量
        /// </summary>
        public AlarmConfig alarmListConfig { get; set; } = new();

        /// <summary>
        /// 设备生产数据读取变量
        /// </summary>
        public DataConfig dataConfig { get; set; } = new();

        /// <summary>
        /// 设备设置变量
        /// </summary>
        public ParameterListConfig parameterListConfig { get; set; } = new();

        /// <summary>
        /// 循环读取变量
        /// </summary>
        public CyclicReadConfig cyclicReadListConfig { get; set; } = new();
    }
}