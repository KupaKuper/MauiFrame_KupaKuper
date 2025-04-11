namespace KupaKuper_IO.PlcConfig
{
    public class IoLConfig
    {
        public List<Io> InputIoList { get; set; } = new() { new(), new(), new(), new(), new() };
        public List<Io> OutputIoList { get; set; } = new() { new(), new(), new(), new(), new() };
    }
}