namespace KupaKuper_MauiControl.Resources
{
    /// <summary>
    /// 自定义样式资源字典的代码后置类
    /// </summary>
    public partial class MyStyles : ResourceDictionary
    {
        /// <summary>
        /// 构造函数：初始化样式资源字典
        /// </summary>
        public MyStyles()
        {
            try
            {
                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("自定义样式资源字典初始化完成");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化样式资源字典时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定键的资源值
        /// </summary>
        /// <param name="key">资源键</param>
        /// <returns>资源值，如果未找到则返回null</returns>
        public object GetResource(string key)
        {
            try
            {
                if (TryGetValue(key, out var value))
                {
                    return value;
                }
                System.Diagnostics.Debug.WriteLine($"未找到资源键: {key}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取资源值时出错: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 动态添加或更新资源
        /// </summary>
        /// <param name="key">资源键</param>
        /// <param name="value">资源值</param>
        public void SetResource(string key, object value)
        {
            try
            {
                if (ContainsKey(key))
                {
                    this[key] = value;
                    System.Diagnostics.Debug.WriteLine($"更新资源: {key}");
                }
                else
                {
                    Add(key, value);
                    System.Diagnostics.Debug.WriteLine($"添加新资源: {key}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置资源值时出错: {ex.Message}");
            }
        }
    }
}

