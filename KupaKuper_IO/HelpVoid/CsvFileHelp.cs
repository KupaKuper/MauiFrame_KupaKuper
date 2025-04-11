using System.Text;

namespace KupaKuper_IO.HelpVoid
{
    public static class CsvFileHelper
    {
        // 文件锁对象
        private static readonly object _fileLock = new object();

        /// <summary>
        /// 向当前文档追加一行文本
        /// </summary>
        /// <param name="CsvAddress">CSV文件路径</param>
        /// <param name="txt">要追加的文本</param>
        public static void AddNewLine(string CsvAddress, string txt)
        {
            if (string.IsNullOrEmpty(txt)) return;

            try
            {
                lock (_fileLock)
                {
                    // 使用 FileStream 以追加模式打开文件
                    using (var fs = new FileStream(CsvAddress, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (var sw = new StreamWriter(fs, EncodingHelp.GetEncoding("GBK")))
                    {
                        sw.WriteLine(txt);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CSV写入错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 向当前文档追加多行文本
        /// </summary>
        /// <param name="CsvAddress">CSV文件路径</param>
        /// <param name="txt">要追加的文本列表</param>
        public static void AddNewLine(string CsvAddress, List<string> txt)
        {
            if (txt == null || txt.Count == 0) return;

            try
            {
                lock (_fileLock)
                {
                    // 使用 BufferedStream 提高写入性能
                    using (var fs = new FileStream(CsvAddress, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (var bs = new BufferedStream(fs, 65536)) // 64KB buffer
                    using (var sw = new StreamWriter(bs, EncodingHelp.GetEncoding("GBK")))
                    {
                        foreach (var line in txt)
                        {
                            if (!string.IsNullOrEmpty(line))
                            {
                                sw.WriteLine(line);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CSV批量写入错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 异步向当前文档追加多行文本
        /// </summary>
        /// <param name="CsvAddress">CSV文件路径</param>
        /// <param name="txt">要追加的文本列表</param>
        public static async Task AddNewLineAsync(string CsvAddress, List<string> txt)
        {
            if (txt == null || txt.Count == 0) return;

            try
            {
                await Task.Run(() =>
                {
                    lock (_fileLock)
                    {
                        using (var fs = new FileStream(CsvAddress, FileMode.Append, FileAccess.Write, FileShare.Read))
                        using (var bs = new BufferedStream(fs, 65536))
                        using (var sw = new StreamWriter(bs, EncodingHelp.GetEncoding("GBK")))
                        {
                            foreach (var line in txt)
                            {
                                if (!string.IsNullOrEmpty(line))
                                {
                                    sw.WriteLine(line);
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CSV异步批量写入错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建新的CSV文件
        /// </summary>
        /// <param name="CsvAddress">CSV文件路径</param>
        /// <param name="headers">CSV表头</param>
        public static void CreateNewCsvFile(string CsvAddress, List<string> headers)
        {
            try
            {
                lock (_fileLock)
                {
                    // 确保目录存在
                    var directory = Path.GetDirectoryName(CsvAddress);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // 创建新文件并写入表头
                    using (var fs = new FileStream(CsvAddress, FileMode.Create, FileAccess.Write, FileShare.Read))
                    using (var sw = new StreamWriter(fs, EncodingHelp.GetEncoding("GBK")))
                    {
                        sw.WriteLine(string.Join(",", headers));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CSV文件创建错误: {ex.Message}");
            }
        }
        /// <summary>
        /// 读取CSV文件的所有行
        /// </summary>
        /// <param name="CsvAddress">CSV文件路径</param>
        /// <param name="skipHeader">是否跳过表头</param>
        /// <returns>字符串列表，每个字符串代表一行数据</returns>
        public static async Task<List<string>> ReadAllLinesAsync(string CsvAddress, bool skipHeader = false)
        {
            var results = new List<string>();
            if (!File.Exists(CsvAddress)) return results;

            const int maxRetries = 3;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    lock (_fileLock)
                    {
                        using var fs = new FileStream(CsvAddress, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using var bs = new BufferedStream(fs, 65536); // 64KB buffer
                        using var reader = new StreamReader(bs, EncodingHelp.GetEncoding("GBK"));

                        // 如果需要跳过表头
                        if (skipHeader)
                        {
                            reader.ReadLine();
                        }

                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                results.Add(line);
                            }
                        }
                    }
                    break; // 读取成功，跳出重试循环
                }
                catch (IOException)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        System.Diagnostics.Debug.WriteLine($"读取CSV文件失败，已重试{maxRetries}次");
                        break;
                    }
                    await Task.Delay(100 * retryCount); // 递增延迟重试
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"读取CSV文件发生未知错误: {ex.Message}");
                    break;
                }
            }

            return results;
        }

        /// <summary>
        /// 读取CSV文件的所有行并按分隔符拆分
        /// </summary>
        /// <param name="CsvAddress">CSV文件路径</param>
        /// <param name="separator">分隔符，默认为逗号</param>
        /// <param name="skipHeader">是否跳过表头</param>
        /// <returns>字符串数组的列表，每个数组代表一行拆分后的数据</returns>
        public static async Task<List<string[]>> ReadAllLinesAndSplitAsync(string CsvAddress, char separator = ',', bool skipHeader = false)
        {
            var results = new List<string[]>();
            if (!File.Exists(CsvAddress)) return results;

            const int maxRetries = 3;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    lock (_fileLock)
                    {
                        using var fs = new FileStream(CsvAddress, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using var bs = new BufferedStream(fs, 65536); // 64KB buffer
                        using var reader = new StreamReader(bs, EncodingHelp.GetEncoding("GBK"));

                        // 如果需要跳过表头
                        if (skipHeader)
                        {
                            reader.ReadLine();
                        }

                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                results.Add(line.Split(separator));
                            }
                        }
                    }
                    break; // 读取成功，跳出重试循环
                }
                catch (IOException)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        System.Diagnostics.Debug.WriteLine($"读取CSV文件失败，已重试{maxRetries}次");
                        break;
                    }
                    await Task.Delay(100 * retryCount); // 递增延迟重试
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"读取CSV文件发生未知错误: {ex.Message}");
                    break;
                }
            }

            return results;
        }
        /// <summary>
        /// 读取CSV文件的指定一行数据
        /// </summary>
        /// <param name="CsvAddress">CSV文件路径</param>
        /// <param name="lineNumber">要读取的行号（从1开始），如果为0则忽略该参数</param>
        /// <param name="headerContains">表头包含的字符串，如果不为空则按此条件读取</param>
        /// <returns>字符串数组，代表指定行的数据</returns>
        public static async Task<string[]> ReadSpecificLineAsync(string CsvAddress, int lineNumber = 0, string headerContains = "")
        {
            if (!File.Exists(CsvAddress)) return new string[0];

            const int maxRetries = 3;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    lock (_fileLock)
                    {
                        using var fs = new FileStream(CsvAddress, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using var bs = new BufferedStream(fs, 65536); // 64KB buffer
                        // 修改为GBK编码
                        using var reader = new StreamReader(bs, EncodingHelp.GetEncoding("GBK"));

                        int currentLine = 1;
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (lineNumber > 0 && currentLine == lineNumber)
                            {
                                // 修改为返回删除第一个元素后的新数组
                                var parts = line.Split(',');
                                if (parts.Length > 1)
                                {
                                    var newArray = new string[parts.Length - 1];
                                    Array.Copy(parts, 1, newArray, 0, parts.Length - 1);
                                    return newArray;
                                }
                                else
                                {
                                    return new string[0];
                                }
                            }
                            if (!string.IsNullOrEmpty(headerContains) && currentLine == 1 && line.Contains(headerContains))
                            {
                                // 修改为返回删除第一个元素后的新数组
                                var parts = line.Split(',');
                                if (parts.Length > 1)
                                {
                                    var newArray = new string[parts.Length - 1];
                                    Array.Copy(parts, 1, newArray, 0, parts.Length - 1);
                                    return newArray;
                                }
                                else
                                {
                                    return new string[0];
                                }
                            }
                            currentLine++;
                        }
                    }
                    break; // 读取成功，跳出重试循环
                }
                catch (IOException)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        System.Diagnostics.Debug.WriteLine($"读取CSV文件指定行失败，已重试{maxRetries}次");
                        break;
                    }
                    await Task.Delay(100 * retryCount); // 递增延迟重试
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"读取CSV文件指定行发生未知错误: {ex.Message}");
                    break;
                }
            }

            return new string[0];
        }

        /// <summary>
        /// 读取CSV文件的指定一列数据
        /// </summary>
        /// <param name="CsvAddress">CSV文件路径</param>
        /// <param name="columnNumber">要读取的列号（从1开始），如果为0则忽略该参数</param>
        /// <param name="headerContains">表头包含的字符串，如果不为空则按此条件读取</param>
        /// <returns>字符串列表，代表指定列的数据</returns>
        public static async Task<List<string>> ReadSpecificColumnAsync(string CsvAddress, int columnNumber = 0, string headerContains = "")
        {
            var results = new List<string>();
            if (!File.Exists(CsvAddress)) return results;

            const int maxRetries = 3;
            int retryCount = 0;
            int targetColumn = -1;

            while (retryCount < maxRetries)
            {
                try
                {
                    lock (_fileLock)
                    {
                        using var fs = new FileStream(CsvAddress, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using var bs = new BufferedStream(fs, 65536); // 64KB buffer
                        // 修改为GBK编码
                        using StreamReader reader = new(CsvAddress, EncodingHelp.GetEncoding("GBK"));

                        string? line;
                        bool isHeader = true;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var values = line.Split(',');
                            if (isHeader)
                            {
                                if (columnNumber > 0 && columnNumber <= values.Length)
                                {
                                    targetColumn = columnNumber - 1;
                                }
                                else if (!string.IsNullOrEmpty(headerContains))
                                {
                                    for (int i = 0; i < values.Length; i++)
                                    {
                                        if (values[i].Contains(headerContains))
                                        {
                                            targetColumn = i;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (!isHeader && targetColumn >= 0 && targetColumn < values.Length)
                            {
                                results.Add(values[targetColumn]);
                            }
                            isHeader = false;
                        }
                    }
                    break; // 读取成功，跳出重试循环
                }
                catch (IOException)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        System.Diagnostics.Debug.WriteLine($"读取CSV文件指定列失败，已重试{maxRetries}次");
                        break;
                    }
                    await Task.Delay(100 * retryCount); // 递增延迟重试
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"读取CSV文件指定列发生未知错误: {ex.Message}");
                    break;
                }
            }

            return results;
        }
    }
    public static class EncodingHelp
    {

        /// <summary>
        /// 获取指定类型的编码格式
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Encoding GetEncoding(string name)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding(name);
        }
    }
}
