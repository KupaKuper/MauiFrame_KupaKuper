using System.Globalization;

namespace KupaKuper_HmiView.Converters
{
    /// <summary>
    /// 过滤类型文本颜色转换器：根据选中状态返回不同的文本颜色
    /// </summary>
    public class FilterTypeToTextColorConverter : IValueConverter
    {
        /// <summary>
        /// 将过滤类型转换为文本颜色
        /// </summary>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is KupaKuper_HmiView.ContentViewModes.AlarmFilterType selectedType && parameter is string _targetType)
            {
                bool isSelected = selectedType.ToString() == _targetType;
                return isSelected ? Colors.Black : Colors.Gray; // 选中显示黑色，未选中显示灰色
            }
            return Colors.Gray;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
