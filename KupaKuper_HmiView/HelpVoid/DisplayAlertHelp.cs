namespace KupaKuper_HmiView.HelpVoid
{
    public static class DisplayAlertHelp
    {
        // 添加静态字段用于管理弹窗
        private static readonly object _alertLock = new object();
        private static bool _isDisplayingAlert = false;
        private static readonly Queue<(string title, string message, string accept, string? cancel, string? placeholder, string initialValue, int maxLength, Keyboard? keyboard, Func<string, bool>? validation)> _alertQueue = new();
        private static readonly SemaphoreSlim _alertSemaphore = new(1, 1);

        /// <summary>
        /// 使用lock锁的输入型弹窗,防止多次弹窗报错，实现弹窗排队
        /// </summary>
        /// <param name="title">弹窗标题</param>
        /// <param name="message">弹窗信息</param>
        /// <param name="accept">确认按钮文本</param>
        /// <param name="cancel">取消按钮文本</param>
        /// <param name="placeholder">输入框占位符</param>
        /// <param name="initialValue">输入框初始值</param>
        /// <param name="maxLength">输入框最大长度</param>
        /// <param name="keyboard">键盘类型</param>
        /// <param name="validation">输入验证方法</param>
        /// <returns>包含用户输入的任务</returns>
        public async static Task<string> TryDisplayPromptAsync(string title, string message, string accept = "OK", string? cancel = "Cancel", string? placeholder = null, string initialValue = "", int maxLength = -1, Keyboard? keyboard = null, Func<string, bool>? validation = null)
        {
            // 将弹窗信息加入队列
            lock (_alertLock)
            {
                _alertQueue.Enqueue((title, message, accept, cancel, placeholder, initialValue, maxLength, keyboard, validation));
            }

            // 尝试显示弹窗
            return await ShowNextPromptAsync();
        }

        /// <summary>
        /// 显示下一个输入型弹窗
        /// </summary>
        private static async Task<string> ShowNextPromptAsync()
        {
            // 使用信号量确保同一时间只有一个线程在处理队列
            await _alertSemaphore.WaitAsync();

            try
            {
                // 如果已经在显示弹窗，直接返回
                if (_isDisplayingAlert)
                {
                    return null;
                }

                // 获取下一个弹窗信息
                (string title, string message, string accept, string? cancel, string? placeholder, string initialValue, int maxLength, Keyboard? keyboard, Func<string, bool>? validation) alertInfo;
                lock (_alertLock)
                {
                    if (!_alertQueue.Any())
                    {
                        return null;
                    }
                    alertInfo = _alertQueue.Dequeue();
                }

                try
                {
                    _isDisplayingAlert = true;

                    // 显示输入型弹窗
                    if (Application.Current?.MainPage != null)
                    {
                        return await Application.Current.MainPage.DisplayPromptAsync(
                            alertInfo.title,
                            alertInfo.message,
                            alertInfo.accept,
                            alertInfo.cancel,
                            alertInfo.placeholder,
                            alertInfo.maxLength,
                            alertInfo.keyboard,
                            alertInfo.initialValue
                        );
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"显示输入型弹窗时出错: {ex.Message}");
                }
                finally
                {
                    _isDisplayingAlert = false;

                    // 检查是否还有待显示的弹窗
                    lock (_alertLock)
                    {
                        if (_alertQueue.Any())
                        {
                            // 在主线程中显示下一个弹窗
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await ShowNextPromptAsync();
                            });
                        }
                    }
                }
            }
            finally
            {
                _alertSemaphore.Release();
            }

            return null;
        }

        /// <summary>
        /// 使用lock锁的弹窗,防止多次弹窗报错，实现弹窗排队
        /// </summary>
        /// <param name="title">弹窗标题</param>
        /// <param name="message">弹窗信息</param>
        /// <param name="accept">取消按钮文本</param>
        /// <returns>弹窗任务</returns>
        public async static Task TryDisplayAlert(string title, string message, string accept)
        {
            // 将弹窗信息加入队列
            lock (_alertLock)
            {
                _alertQueue.Enqueue((title, message, accept, null, null, "", -1, null, null));
            }

            // 尝试显示弹窗
            await ShowNextAlert();
        }

        /// <summary>
        /// 显示下一个弹窗
        /// </summary>
        private static async Task ShowNextAlert()
        {
            // 使用信号量确保同一时间只有一个线程在处理队列
            await _alertSemaphore.WaitAsync();

            try
            {
                // 如果已经在显示弹窗，直接返回
                if (_isDisplayingAlert)
                {
                    return;
                }

                // 获取下一个弹窗信息
                (string title, string message, string accept, string? cancel, string? placeholder, string initialValue, int maxLength, Keyboard? keyboard, Func<string, bool>? validation) alertInfo;
                lock (_alertLock)
                {
                    if (!_alertQueue.Any())
                    {
                        return;
                    }
                    alertInfo = _alertQueue.Dequeue();
                }

                try
                {
                    _isDisplayingAlert = true;

                    // 显示弹窗
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            alertInfo.title,
                            alertInfo.message,
                            alertInfo.accept
                        );
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"显示弹窗时出错: {ex.Message}");
                }
                finally
                {
                    _isDisplayingAlert = false;

                    // 检查是否还有待显示的弹窗
                    lock (_alertLock)
                    {
                        if (_alertQueue.Any())
                        {
                            // 在主线程中显示下一个弹窗
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await ShowNextAlert();
                            });
                        }
                    }
                }
            }
            finally
            {
                _alertSemaphore.Release();
            }
        }

        /// <summary>
        /// 清空弹窗队列
        /// </summary>
        public static void ClearAlertQueue()
        {
            lock (_alertLock)
            {
                _alertQueue.Clear();
            }
        }

        /// <summary>
        /// 获取当前队列中的弹窗数量
        /// </summary>
        public static int PendingAlertCount
        {
            get
            {
                lock (_alertLock)
                {
                    return _alertQueue.Count;
                }
            }
        }
    }
}
