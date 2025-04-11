using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KupaKuper_MauiControl.ControlModes
{
    public partial class CsvTableViewMode : ObservableObject
    {
        #region 属性
        [ObservableProperty]
        private bool isEditable;

        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private bool isSearchOperation;

        [ObservableProperty]
        private ObservableCollection<ColumnDefinition> columnDefinitions = new();

        [ObservableProperty]
        private ObservableCollection<TableRow> rows = new();

        [ObservableProperty]
        private ObservableCollection<TableRow> filteredRows = new();

        [ObservableProperty]
        private ObservableCollection<KeyValuePair<string, double>> columnWidths = new();

        [ObservableProperty]
        private float tableWidth;

        [ObservableProperty]
        private float tableHeight;

        [ObservableProperty]
        private bool hasChanges;

        [ObservableProperty]
        private double fontSize = 14; // 默认字体大小

        [ObservableProperty]
        private double cellPadding = 5; // 默认单元格内边距

        private CsvTableDataMode bindingData;
        private double minColumnWidth = 100; // 最小列宽
        private double minAdaptiveColumnWidth = 60; // 自适应模式下的最小列宽
        private int rowIndex = 0;
        private const int MaxNormalColumns = 9; // 超过这个数量的列将触发自适应模式

        /// <summary>
        /// 保存上一次搜索的文本，用于判断是新搜索还是搜索清除
        /// </summary>
        public string LastSearchText { get; private set; } = string.Empty;

        public Color AlternatingRowColor
        {
            get
            {
                rowIndex++;
                return rowIndex % 2 == 0 
                    ? Colors.Transparent 
                    : Application.Current?.RequestedTheme == AppTheme.Dark 
                        ? Color.FromArgb("#1A1A1A") 
                        : Color.FromArgb("#F8F9FA");
            }
        }

        private Dictionary<string, string> CurrentRow { get; set; }
        #endregion

        #region 命令
        [RelayCommand]
        private void Search()
        {
            // 保存上一次搜索文本
            LastSearchText = SearchText;
            
            ObservableCollection<TableRow> filtered;
            
            if (string.IsNullOrEmpty(SearchText))
            {
                // 搜索被清除，显示全部数据
                filtered = new ObservableCollection<TableRow>(Rows);
            }
            else
            {
                // 执行搜索过滤
                filtered = new ObservableCollection<TableRow>(
                    Rows.Where(row => 
                        row.Cells.Any(cell => 
                            cell.Value != null && cell.Value.Contains(SearchText, StringComparison.OrdinalIgnoreCase))));
            }
            
            // 更新过滤数据
            FilteredRows = filtered;
            
            // 确保发出通知，以便视图能够更新
            OnPropertyChanged(nameof(FilteredRows));
        }

        [RelayCommand]
        private Task SaveChanges()
        {
            UpdateBindingData();
            HasChanges = false;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 手动刷新数据源
        /// </summary>
        [RelayCommand]
        private void RefreshData()
        {
            if (bindingData == null) return;
            
            // 重新加载数据但保持过滤状态
            LoadAllData();
            
            // 如果有搜索条件，重新应用搜索
            if (!string.IsNullOrEmpty(SearchText))
            {
                Search();
            }
        }
        #endregion

        #region 数据处理方法
        private void UpdateBindingData()
        {
            if (bindingData == null) return;

            bindingData.Headers = Rows.Select(r => r.Cells.FirstOrDefault(c => c.Width > 0).Value).ToArray();
            bindingData.Rows = Rows.Select(r => r.Cells.Select(c => c.Value).ToArray()).ToList();
        }

        public void SetBindingData(CsvTableDataMode data, double availableWidth)
        {
            if (data == null || data.Headers == null || data.Rows == null)
            {
                return;
            }
            
            bindingData = data;
            
            // 清空现有数据
            Rows.Clear();
            FilteredRows.Clear();
            ColumnDefinitions.Clear();
            
            // 设置列定义
            SetupColumns(data.Headers, availableWidth);
            
            // 加载全部数据
            LoadAllData();
        }

        private void LoadAllData()
        {
            // 确保有数据可加载
            if (bindingData == null || bindingData.Rows == null || bindingData.Rows.Count == 0)
            {
                return;
            }

            var tableRows = new List<TableRow>();
            rowIndex = 0;

            // 加载所有行数据
            for (int i = 0; i < bindingData.Rows.Count; i++)
            {
                var rowData = bindingData.Rows[i];
                var cells = new List<TableCell>();
                for (int j = 0; j < Math.Min(bindingData.Headers.Length, rowData.Length); j++)
                {
                    cells.Add(new TableCell
                    {
                        Value = rowData[j],
                        Width = ColumnDefinitions[j].Width,
                        FontSize = FontSize,
                        Padding = CellPadding
                    });
                }

                var rowColor = rowIndex % 2 == 0
                    ? Colors.Transparent
                    : Application.Current.RequestedTheme == AppTheme.Dark
                        ? Color.FromArgb("#1A1A1A")
                        : Color.FromArgb("#F8F9FA");

                tableRows.Add(new TableRow
                {
                    Cells = cells,
                    RowColor = rowColor
                });

                rowIndex++;
            }

            // 在主线程更新 UI
            MainThread.BeginInvokeOnMainThread(() => {
                Rows = new ObservableCollection<TableRow>(tableRows);
                FilteredRows = new ObservableCollection<TableRow>(tableRows);
                
                // 通知UI更新
                OnPropertyChanged(nameof(FilteredRows));
            });
        }

        private void SetupColumns(string[] headers, double availableWidth)
        {
            if (headers == null || headers.Length == 0) return;
            
            var columnCount = headers.Length;
            // 计算所有表头字符串的总长度
            var totalHeaderLength = headers.Sum(header => header.Length);
            
            // 根据列数量决定是否启用自适应模式
            bool adaptiveMode = columnCount > MaxNormalColumns;
            
            // 计算字体大小和内边距
            if (adaptiveMode)
            {
                // 根据列数量动态调整字体大小和内边距
                FontSize = Math.Max(8, 14 - (columnCount - MaxNormalColumns) * 0.5);
                CellPadding = Math.Max(2, 5 - (columnCount - MaxNormalColumns) * 0.3);
            }
            else
            {
                // 正常模式
                FontSize = 14;
                CellPadding = 5;
            }

            var newColumnDefinitions = new ObservableCollection<ColumnDefinition>();
            double totalCalculatedWidth = 0;
            foreach (var header in headers)
            {
                // 根据表头字符串长度占总长度的比例来分配列宽
                var columnWidth = Math.Max(adaptiveMode ? minAdaptiveColumnWidth : minColumnWidth, (availableWidth * header.Length) / totalHeaderLength);
                newColumnDefinitions.Add(new ColumnDefinition
                {
                    Title = header,
                    Width = columnWidth,
                    FontSize = FontSize,
                    Padding = CellPadding
                });
                totalCalculatedWidth += columnWidth;
            }

            // 如果总计算宽度超过可用宽度，按比例缩小列宽
            if (totalCalculatedWidth > availableWidth)
            {
                var ratio = availableWidth / totalCalculatedWidth;
                foreach (var colDef in newColumnDefinitions)
                {
                    colDef.Width *= ratio;
                }
            }

            ColumnDefinitions = newColumnDefinitions;
        }

        /// <summary>
        /// 重新计算列宽
        /// </summary>
        public void RecalculateColumnWidths(double availableWidth)
        {
            if (ColumnDefinitions == null || ColumnDefinitions.Count == 0) return;

            // 计算已设置固定宽度的列的总宽度和数量
            double fixedWidth = 0;
            int flexibleColumnCount = 0;
            double totalFlexibleHeaderLength = 0;
            var flexibleHeaders = new List<string>();

            foreach (var colDef in ColumnDefinitions)
            {
                if (colDef.IsFixedWidth)
                {
                    fixedWidth += colDef.Width;
                }
                else
                {
                    flexibleColumnCount++;
                    flexibleHeaders.Add(colDef.Title);
                    totalFlexibleHeaderLength += colDef.Title.Length;
                }
            }

            // 计算剩余的可用宽度
            double remainingWidth = Math.Max(0, availableWidth - fixedWidth);

            // 重新分配弹性列的宽度
            if (flexibleColumnCount > 0)
            {
                double totalCalculatedFlexibleWidth = 0;
                for (int i = 0; i < ColumnDefinitions.Count; i++)
                {
                    var colDef = ColumnDefinitions[i];
                    if (!colDef.IsFixedWidth)
                    {
                        var header = flexibleHeaders[i - (ColumnDefinitions.Count - flexibleColumnCount)];
                        var columnWidth = Math.Max(minColumnWidth, (remainingWidth * header.Length) / totalFlexibleHeaderLength);
                        colDef.Width = columnWidth;
                        totalCalculatedFlexibleWidth += columnWidth;
                    }
                }

                // 如果总计算宽度超过剩余可用宽度，按比例缩小列宽
                if (totalCalculatedFlexibleWidth > remainingWidth)
                {
                    var ratio = remainingWidth / totalCalculatedFlexibleWidth;
                    foreach (var colDef in ColumnDefinitions)
                    {
                        if (!colDef.IsFixedWidth)
                        {
                            colDef.Width *= ratio;
                        }
                    }
                }
            }

            // 通知属性变更以更新UI
            OnPropertyChanged(nameof(ColumnDefinitions));
        }
        #endregion

        #region 键值转换器
        public double GetColumnWidth(string columnName)
        {
            var column = ColumnWidths.FirstOrDefault(c => c.Key == columnName);
            return column.Value > 0 ? column.Value : minColumnWidth;
        }
        #endregion

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            
            // 当搜索文本变化时自动触发搜索
            if (e.PropertyName == nameof(SearchText))
            {
                SearchCommand.Execute(null);
            }
        }
    }

    public partial class ColumnDefinition : ObservableObject
    {
        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private double width;
        
        [ObservableProperty]
        private double fontSize = 14;
        
        [ObservableProperty]
        private double padding = 5;

        [ObservableProperty]
        private bool isFixedWidth;
    }

    public partial class TableCell : ObservableObject
    {
        [ObservableProperty]
        private string value;

        [ObservableProperty]
        private double width;
        
        [ObservableProperty]
        private double fontSize = 14;
        
        [ObservableProperty]
        private double padding = 5;
    }

    public partial class TableRow : ObservableObject
    {
        [ObservableProperty]
        private List<TableCell> cells = new();

        [ObservableProperty]
        private Color rowColor;
    }

    // 键值转换器，用于绑定
    public class KeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is string key && value is CsvTableViewMode viewModel)
            {
                return viewModel.GetColumnWidth(key);
            }
            return 100; // 默认宽度
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
