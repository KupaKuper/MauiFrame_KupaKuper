using System.Globalization;

namespace KupaKuper_MauiControl.Converters
{
    /// <summary>
    /// 布尔值转换为颜色的转换器
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        /// <summary>
        /// 将布尔值转换为颜色
        /// </summary>
        /// <param name="value">布尔值</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">可选参数，可以传入自定义颜色字符串，格式："TrueColor,FalseColor"</param>
        /// <param name="culture">区域信息</param>
        /// <returns>转换后的颜色</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (parameter is string paramString)
                {
                    // 参数格式: "TrueColor,FalseColor"
                    var colors = paramString.Split(',');
                    if (colors.Length >= 2)
                    {
                        string colorKey = boolValue ? colors[0] : colors[1];

                        // 处理特殊值
                        if (colorKey == "Transparent")
                            return Colors.Transparent;
                        if (colorKey == "Default")
                            return null; // 返回null让系统使用默认颜色
                        if (colorKey == "White")
                            return Colors.White;

                        // 尝试从资源字典获取颜色
                        var color = new object();
                        if ((bool)Application.Current?.Resources.TryGetValue(colorKey, out color))
                        {
                            return color;
                        }
                    }
                }
                return boolValue ? Colors.Orange : Colors.Gray;
            }
            // 默认返回透明色
            return Colors.Transparent;
        }

        /// <summary>
        /// 将颜色转换回布尔值（通常不需要实现）
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}