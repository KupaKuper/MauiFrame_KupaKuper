using KupaKuper_IO.Ethernet;

namespace KupaKuper_IO.PlcConfig
{
    public partial class Parameter
    {
        /// <summary>
        /// 参数描述
        /// </summary>
        public string Name { get; set; } = "参数名称";

        /// <summary>
        /// 参数变量
        /// </summary>
        public PlcVar plcVar { get; set; } = new() { VarInfo = "变量名" };
    }
}