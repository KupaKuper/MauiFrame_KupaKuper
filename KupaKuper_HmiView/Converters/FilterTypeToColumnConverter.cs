using System.Globalization;

namespace KupaKuper_HmiView.Converters
{
    /// <summary>
    /// 过滤类型列转换器：将过滤类型转换为对应的列索引
    /// </summary>
    public class FilterTypeToColumnConverter : IValueConverter
    {
        /// <summary>
        /// 将过滤类型转换为列索引
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is KupaKuper_HmiView.ContentViewModes.AlarmFilterType filterType)
            {
                // 直接使用枚举值
                return filterType;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 