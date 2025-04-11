using Core.Net.Inovance;
using System.Text;

namespace KupaKuper_IO.Ethernet.PlcEthernet
{
    public class Inovance_Ethernet : BasePlcEthernet
    {
        private string _serverUrl;
        private EthernetWrapper? _client;
        public Inovance_Ethernet()
        {
            _serverUrl = ClientUrl;
        }
        public override object Read(string trg)
        {
            uint.TryParse(trg, out uint address);
            if (!Connected) Reconnect();
            Connected = false;
            if (_client is null) throw new NullReferenceException();
            var result = _client.Read<EthernetType>(address);
            Connected = true;
            return result;
        }

        public override string ReadString(string trg, string encoding)
        {
            var value = Read(trg);
            if (value is not byte[] temp) return "";
            var conding = Encoding.GetEncoding(encoding);
            var result = conding.GetString(temp).Replace("\0", "");
            return result;
        }

        public override T Read<T>(string trg)
        {
            uint.TryParse(trg, out uint address);
            if (!Connected) Reconnect();
            Connected = false;
            if (_client is null) throw new NullReferenceException();
            var result = _client.Read(address,typeof(T));
            Connected = true;
            return (T)result;
        }

        public override void Write(string trg, object value)
        {
            if (uint.TryParse(trg, out uint address)) return;
            if (!Connected) Reconnect();
            Connected = false;
            if (_client is null) throw new NullReferenceException();
            _client.Write(address, value, value.GetType());
            Connected = true;
        }

        public override void Reconnect()
        {
            Close();
            Connect();
        }

        public override void Close()
        {
            Connected = false;
            _client?.TcpClient.Close();
            _client = null;
        }

        public override void Connect()
        {
            _client = _client ?? new();
            _client.Connect(_serverUrl, 1000);
            Connected = true;
        }

        public override object Read(List<string> addresses)
        {
            if (!Connected) Reconnect();
            if (_client is null) throw new NullReferenceException();

            try
            {
                Connected = false;
                
                // 将字符串地址转换为uint地址列表
                var uintAddresses = addresses
                    .Select(addr => uint.TryParse(addr, out uint result) ? result : 0)
                    .Where(addr => addr != 0)
                    .ToList();

                if (!uintAddresses.Any())
                    return null;

                // 串行读取所有地址
                var results = new List<bool>();
                foreach (var address in uintAddresses)
                {
                    try
                    {
                        var result = _client.Read<bool>(address);
                        results.Add(result);
                    }
                    catch (Exception)
                    {
                        results.Add(false);
                    }
                }

                return results;
            }
            finally
            {
                Connected = true;
            }
        }
    }
}
