using System.Globalization;

namespace KupaKuper_MauiControl.Converters
{
    public partial class ViewIndexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int currentView && parameter is string targetView)
            {
                if (int.TryParse(targetView, out int target))
                {
                    if (Application.Current?.Resources.TryGetValue("blueColor", out var blueColor) == true)
                    {
                        return new SolidColorBrush(currentView == target ? (Color)blueColor : Colors.Transparent);
                    }
                }
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}