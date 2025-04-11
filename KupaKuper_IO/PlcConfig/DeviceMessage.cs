using KupaKuper_IO.Ethernet;

namespace KupaKuper_IO.PlcConfig
{
    public class DeviceMessage
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviecName { get; set; } = "设备名称";

        /// <summary>
        /// 设备通讯类型
        /// </summary>
        public PlcModel DeviceType { get; set; } = PlcModel.OpcUa;

        /// <summary>
        /// 设备Ip地址
        /// </summary>
        public string DeviceAddress { get; } = "192.168.1.1";

        /// <summary>
        /// 设备名称前缀
        /// </summary>
        public string DeviceVarFirstName { get; set; } = "ns=4;s=|var|Inovance-PLC.Application.";
    }
}