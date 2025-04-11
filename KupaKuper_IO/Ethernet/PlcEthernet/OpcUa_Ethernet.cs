using Core.Net.OpcUa;
using System.Text;

namespace KupaKuper_IO.Ethernet.PlcEthernet
{
    public partial class OpcUa_Ethernet:BasePlcEthernet
    {
        private string _serverUrl;
        private OpcUaClientWrapper? _client;
        public OpcUa_Ethernet()
        {
            _serverUrl = ClientUrl;
        }
        public override object Read(string trg)
        {
            if (!Connected) Reconnect();
            Connected = false;
            if (_client is null) throw new NullReferenceException();
            var result = _client.ReadNode(trg);
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
            if (!Connected) Reconnect();
            Connected = false;
            if (_client is null) throw new NullReferenceException();
            var result = _client.ReadNode<T>(trg);
            Connected = true;
            return result;
        }
        public override void Write(string trg, object value)
        {
            if (!Connected) Reconnect();
            Connected = false;
            if (_client is null) throw new NullReferenceException();
            _client.WriteNode(trg, value);
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
            _client?.Disconnect();
            _client = null;
        }
        public override void Connect()
        {
            _client = _client ?? new();
            _client.Connect(_serverUrl, 1000);
            Connected = true;
        }
        public override object Read(List<string> trg)
        {
            if (!Connected) Reconnect();
            Connected = false;
            if (_client is null) throw new NullReferenceException();
            List<object> result = _client.ReadNodes(trg.ToArray());
            Connected = true;
            return result;
        }
    }
}
