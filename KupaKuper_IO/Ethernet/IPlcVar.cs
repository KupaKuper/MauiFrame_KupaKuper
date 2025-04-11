namespace KupaKuper_IO.Ethernet
{
    public enum VarMode
    {
        Bool = TypeCode.Boolean,
        Int16 = TypeCode.Int16,
        Int32 = TypeCode.Int32,
        Float = TypeCode.Single
    }
    public class PlcVar
    {
        public string VarInfo { get; set; } = "变量简介";
        /// <summary>
        /// PlC中的变量名
        /// </summary>
        public string PlcVarName { get; set; } = "";
        /// <summary>
        /// PlC中的变量读取地址
        /// </summary>
        public string PlcVarAddress { get; set; } = "";
        /// <summary>
        /// PLC中的变量类型
        /// </summary>
        public VarMode PlcVarMode { get; set; }
    }
}
