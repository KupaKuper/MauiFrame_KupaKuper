using System.Diagnostics;
using System.Collections.Concurrent;

namespace KupaKuper_IO.HelpVoid
{
    //使用方法
    // var configs = new List<FileWatcherConfig>
    //{
    //    new FileWatcherConfig 
    //    {
    //        Enable = true,
    //        FileSystemWatcher = new FileSystemWatcher
    //        {
    //            Path = @".\AlarmLog",
    //            Filter = "*.csv"
    //        }
    //    }
    //};
    //using var fileWatcher = new FileWatcher(configs);
    //fileWatcher.FileChanged += (filePath) =>
    //{
    //    Debug.WriteLine($"文件已更改: {filePath}");
    //    // 处理文件变更
    //};

    /// <summary>
    /// 文件监控类
    /// </summary>
    public class FileWatcher : IDisposable
    {
        private readonly ConcurrentDictionary<FileSystemWatcher, FileWatcherConfig> _watchers = new();
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
        private readonly ConcurrentDictionary<string, DateTime> _lastProcessedTimes = new();
        private const int MinimumProcessingInterval = 1000; // 最小处理间隔（毫秒）

        public delegate void FileWatcheredHandler(string fileAddress);
        /// <summary>
        /// 文件改动事件
        /// </summary>
        public event FileWatcheredHandler FileChanged;

        public FileWatcher(List<FileWatcherConfig> watcherConfigs)
        {
            InitializeWatchers(watcherConfigs);
        }

        private void InitializeWatchers(List<FileWatcherConfig> watcherConfigs)
        {
            foreach (var config in watcherConfigs.Where(c => c.Enable))
            {
                try
                {
                    string fullPath = Path.GetFullPath(config.FileSystemWatcher.Path);
                    if (!Directory.Exists(fullPath))
                    {
                        Debug.WriteLine($"目录不存在: {fullPath}");
                        continue;
                    }

                    var watcher = config.FileSystemWatcher;
                    // 使用异步事件处理
                    watcher.Changed += async (s, e) => await OnChangedAsync(s, e, config);
                    watcher.Error += OnError;
                    watcher.EnableRaisingEvents = true;

                    if (_watchers.TryAdd(watcher, config))
                    {
                        Debug.WriteLine($"开始监控文件夹: {fullPath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"初始化文件监控失败: {ex.Message}");
                }
            }
        }

        private async Task OnChangedAsync(object source, FileSystemEventArgs e, FileWatcherConfig config)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed) return;

            var fileKey = e.FullPath.ToLower();
            var now = DateTime.Now;

            // 检查是否需要处理该文件变更
            if (_lastProcessedTimes.TryGetValue(fileKey, out DateTime lastTime))
            {
                if ((now - lastTime).TotalMilliseconds < MinimumProcessingInterval)
                {
                    return;
                }
            }

            // 更新最后处理时间
            _lastProcessedTimes.AddOrUpdate(fileKey, now, (_, _) => now);

            try
            {
                await _processingSemaphore.WaitAsync();
                await ProcessFileChangeAsync(e.FullPath, config);
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }

        private async Task ProcessFileChangeAsync(string filePath, FileWatcherConfig config)
        {
            var maxRetries = 5;
            var currentRetry = 0;
            var backoffInterval = 1000; // 初始延迟时间（毫秒）

            while (currentRetry < maxRetries)
            {
                try
                {
                    if (await IsFileReadyAsync(filePath))
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        FileChanged?.Invoke(filePath);

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

        private async Task<bool> IsFileReadyAsync(string filePath)
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

        private void OnError(object source, ErrorEventArgs e)
        {
            Debug.WriteLine($"文件监控错误: {e.GetException().Message}");
        }

        public void Dispose()
        {
            foreach (var watcher in _watchers.Keys)
            {
                try
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"释放文件监控资源时出错: {ex.Message}");
                }
            }

            _watchers.Clear();
            _processingSemaphore.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// 文件监控配置类
    /// </summary>
    public class FileWatcherConfig
    {
        /// <summary>
        /// 是否启用监控
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 文件系统监控器配置
        /// </summary>
        public FileSystemWatcher FileSystemWatcher { get; set; } = GetDefaultFileSystemWatcher();

        private static FileSystemWatcher GetDefaultFileSystemWatcher()
        {
            // 这里可以设置一个有效的默认目录路径
            string defaultPath = Directory.GetCurrentDirectory(); 
            return new FileSystemWatcher
            {
                Path = defaultPath,
                Filter = "*.csv",
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.LastWrite
            };
        }
    }
}
