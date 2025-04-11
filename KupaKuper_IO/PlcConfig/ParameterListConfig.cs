namespace KupaKuper_IO.PlcConfig
{
    public class ParameterListConfig
    {
        public List<ParameterList> ParameterList { get; set; } = new() { new() { ListName = "屏蔽参数" }, new() { ListName = "工艺参数" } };
    }
}