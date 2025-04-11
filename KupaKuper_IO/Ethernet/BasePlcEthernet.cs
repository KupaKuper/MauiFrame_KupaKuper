namespace KupaKuper_IO.Ethernet
{
    public abstract class BasePlcEthernet
    {
        /// <summary>
        /// PLC的连接状态
        /// </summary>
        public bool Connected
        {
            get
            {
                return _connected;
            }
            set
            {
                if (_connected != value)
                {
                    _connected = value;
                    if (ConnectErr != null && !_connected) ConnectErr.Invoke();
                }
            }
        }
        private bool _connected = false;
        /// <summary>
        /// PLC的连接地址
        /// </summary>
        public string ClientUrl
        {
            get
            {
                return _clientUrl;
            }
            set
            {
                if (_clientUrl != value)
                {
                    _clientUrl = value;
                }
            }
        }
        private string _clientUrl = "172.1.1.0";
        /// <summary>
        /// PLC的端口号
        /// </summary>
        public string? ClientPort { get; set; }

        public delegate void Plc_ConnectErr();
        /// <summary>
        /// 连接出错时执行的方法
        /// </summary>
        public Plc_ConnectErr? ConnectErr;
        /// <summary>
        /// 读取变量内容
        /// </summary>
        /// <param name="trg"></param>
        /// <returns></returns>
        public abstract Object Read(string trg);
        /// <summary>
        /// 读取一列变量内容
        /// </summary>
        /// <param name="trg"></param>
        /// <returns></returns>
        public abstract Object Read(List<string> trg);
        /// <summary>
        /// 读取字符串变量并指定编码格式
        /// </summary>
        /// <param name="trg"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public abstract string ReadString(string trg, string encoding);
        /// <summary>
        /// 读取指定类型的变量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="trg"></param>
        /// <returns></returns>
        public abstract T Read<T>(string trg);
        /// <summary>
        /// 修改变量内容
        /// </summary>
        /// <param name="trg"></param>
        /// <param name="value"></param>
        public abstract void Write(string trg, object value);
        /// <summary>
        /// 重新连接
        /// </summary>
        public abstract void Reconnect();
        /// <summary>
        /// 关闭连接
        /// </summary>
        public abstract void Close();
        /// <summary>
        /// 连接PLC
        /// </summary>
        public abstract void Connect();

    }
}
