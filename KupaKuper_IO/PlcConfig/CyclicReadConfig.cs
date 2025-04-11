using KupaKuper_IO.Ethernet;

namespace KupaKuper_IO.PlcConfig
{
    public class CyclicReadConfig
    {
        public List<PlcVar> CyclicReadList { get; set; } = new() { new(), new(), new(), new(), new() };
    }
}