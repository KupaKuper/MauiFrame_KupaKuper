using KupaKuper_IO.Ethernet;

namespace KupaKuper_IO.PlcConfig
{
    public class Io
    {
        /// <summary>
        /// Io点名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Io点读取变量
        /// </summary>
        public PlcVar IoVar { get; set; } = new() { VarInfo = "Io点" };
    }
}