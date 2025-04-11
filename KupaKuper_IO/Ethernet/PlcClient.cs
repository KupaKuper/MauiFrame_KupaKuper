using KupaKuper_IO.Ethernet.PlcEthernet;
using KupaKuper_IO.HelpVoid;
using KupaKuper_IO.PlcConfig;

namespace KupaKuper_IO.Ethernet
{
    /// <summary>
    /// PLC类型枚举
    /// </summary>
    public enum PlcModel
    {
        /// <summary>
        /// Opcua协议
        /// </summary>
        OpcUa,
        /// <summary>
        /// 汇川小型专用
        /// </summary>
        Inovance
    }
    /// <summary>
    /// PLC通讯类
    /// </summary>
    public static class PlcClient
    {
        /// <summary>
        /// 读取PLC变量的连接
        /// </summary>
        public static BasePlcEthernet ReadClient;
        /// <summary>
        /// 写PLC变量的连接
        /// </summary>
        public static BasePlcEthernet WriteClient;
        /// <summary>
        /// PLC绑定变量信息
        /// </summary>
        public static DeviceConfig device { get; private set; } = new();

        public static PlcModel plcModel
        {
            get => _plcModel;
            set
            {
                if (value != _plcModel)
                {
                    _plcModel = value;
                    switch (_plcModel)
                    {
                        case PlcModel.Inovance:
                            ReadClient = new Inovance_Ethernet();
                            WriteClient = new Inovance_Ethernet();
                            break;
                        case PlcModel.OpcUa:
                            ReadClient = new OpcUa_Ethernet();
                            WriteClient = new OpcUa_Ethernet();
                            break;
                    }
                }
            }
        }
        private static PlcModel _plcModel = PlcModel.OpcUa;
        /// <summary>
        /// PLC的连接状态
        /// </summary>
        public static bool Plc_Connect
        {
            get
            {
                return plcConnect;
            }
            set
            {
                plcConnect = value;
                if (connectEll != null) connectEll.Invoke();
            }
        }
        private static bool plcConnect = false;
        public delegate void Plc_ConnectEll();
        /// <summary>
        /// 连接出错时执行的方法
        /// </summary>
        public static Plc_ConnectEll? connectEll;
        /// <summary>
        /// PLC通讯类
        /// </summary>
        static PlcClient()
        {
            ReadClient = new OpcUa_Ethernet();
            WriteClient = new OpcUa_Ethernet();
            try
            {
                device = JsonFileHelper.ReadConfig<DeviceConfig>(@".\", "Config");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("读取配置文件出错" + @".\", "Config");
            }
        }

    }
}
