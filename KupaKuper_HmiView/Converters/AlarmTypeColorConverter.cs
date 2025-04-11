using KupaKuper_HmiView.ContentViewModes;

using System.Globalization;

namespace KupaKuper_HmiView.Converters
{
    /// <summary>
    /// 报警类型颜色转换器：将报警类型转换为对应的显示颜色
    /// </summary>
    public class AlarmTypeColorConverter : IValueConverter
    {
        /// <summary>
        /// 将报警类型转换为对应的颜色
        /// </summary>
        /// <param name="value">报警类型值</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">转换参数</param>
        /// <param name="culture">文化信息</param>
        /// <returns>对应的颜色值</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlarmType type)
            {
                return type switch
                {
                    AlarmType.Alarm => Colors.Red,    // 报警显示红色
                    AlarmType.Info => Colors.Orange, // 提示显示橙色
                    _ => Colors.Black                  // 默认显示黑色
                };
            }
            return Colors.Black;
        }

        /// <summary>
        /// 反向转换（不支持）
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 