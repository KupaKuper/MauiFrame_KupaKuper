using System.Diagnostics;

namespace KupaKuper_IO.HelpVoid
{
    public class FileHelp
    {
        public delegate void DelFileDispose(string filePath);
        /// <summary>
        /// 文件操作方法
        /// </summary>
        public event DelFileDispose? FileDispose;
        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetries { get; set; } = 5;
        /// <summary>
        /// 重试间隔
        /// </summary>
        public int BackoffInterval { get; set; } = 1000;
        /// <summary>
        /// 尝试执行文件操作,失败自动重试
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task FileDisposeTask(string filePath)
        {
            var maxRetries = MaxRetries;
            var currentRetry = 0;
            var backoffInterval = BackoffInterval; // 初始延迟时间（毫秒）

            while (currentRetry < maxRetries)
            {
                try
                {
                    if (IsFileReadyAsync(filePath))
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        FileDispose?.Invoke(filePath);

                        stopwatch.Stop();
                        Debug.WriteLine($"文件处理完成: {Path.GetFileName(filePath)}");
                        Debug.WriteLine($"处理耗时: {stopwatch.ElapsedMilliseconds}ms");
                        return;
                    }

                    currentRetry++;
                    await Task.Delay(backoffInterval * currentRetry);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"处理文件变更失败 (尝试 {currentRetry + 1}/{maxRetries}): {ex.Message}");
                    if (currentRetry >= maxRetries - 1) throw;

                    currentRetry++;
                    await Task.Delay(backoffInterval * currentRetry);
                }
            }
        }
        /// <summary>
        /// 获取文件是否正在使用
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool IsFileReadyAsync(string filePath)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
    public static class FileChangeHelp
    {
        /// <summary>
        /// 存放文件最后一次的改动时间
        /// </summary>
        private static Dictionary<string, DateTime> LastFileChangeTime = new();
        /// <summary>
        /// 获取文件是否有修改
        /// </summary>
        /// <param name="FileAdr">文件或文件夹的路径</param>
        /// <returns>如果有修改返回 true，否则返回 false</returns>
        public static bool FileChanged(string FileAdr)
        {
            if (string.IsNullOrEmpty(FileAdr))
            {
                return false;
            }

            if (File.Exists(FileAdr))
            {
                return CheckFileChange(FileAdr);
            }
            else if (Directory.Exists(FileAdr))
            {
                return CheckDirectoryChange(FileAdr);
            }

            return false;
        }

        private static bool CheckFileChange(string filePath)
        {
            DateTime currentLastWriteTime = File.GetLastWriteTimeUtc(filePath);
            if (LastFileChangeTime.TryGetValue(filePath, out DateTime lastWriteTime))
            {
                if (currentLastWriteTime != lastWriteTime)
                {
                    LastFileChangeTime[filePath] = currentLastWriteTime;
                    return true;
                }
                return false;
            }
            else
            {
                LastFileChangeTime[filePath] = currentLastWriteTime;
                return true;
            }
        }

        private static bool CheckDirectoryChange(string directoryPath)
        {
            List<string> currentFiles = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories).ToList();

            // 检查文件删除
            var removedFiles = LastFileChangeTime.Keys.Where(k => k.StartsWith(directoryPath) && !currentFiles.Contains(k)).ToList();
            if (removedFiles.Count > 0)
            {
                foreach (var removedFile in removedFiles)
                {
                    LastFileChangeTime.Remove(removedFile);
                }
                return true;
            }

            // 检查文件新增和修改
            foreach (var file in currentFiles)
            {
                if (CheckFileChange(file))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
