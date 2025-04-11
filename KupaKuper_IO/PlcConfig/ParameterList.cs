namespace KupaKuper_IO.PlcConfig
{
    public class ParameterList
    {
        /// <summary>
        ///
        /// </summary>
        public string ListName { get; set; } = "分类名称";

        public List<Parameter> VarList { get; set; } = new() { new(), new() };
    }
}