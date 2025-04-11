using System.Collections.ObjectModel;

namespace KupaKuper_MauiControl.ControlModes
{
    public class CsvTableGraphicsDrawable : IDrawable
    {
        public ObservableCollection<TableRow> Rows { get; set; } = new();
        public double RowHeight { get; set; } = 40;
        public double ScrollY { get; set; } = 0;
        public double ViewportHeight { get; set; }
        public Dictionary<string, Color> ValueTextColor { get; set; } = new();
        
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Rows == null || Rows.Count == 0)
            {
                return;
            }

            // 计算可见行的范围
            int startRowIndex = Math.Max(0, (int)(ScrollY / RowHeight));
            int visibleRowCount = (int)Math.Ceiling(ViewportHeight / RowHeight) + 1;
            int endRowIndex = Math.Min(Rows.Count - 1, startRowIndex + visibleRowCount);

            // 绘制可见行
            for (int i = startRowIndex; i <= endRowIndex; i++)
            {
                var row = Rows[i];
                float y = (float)(i * RowHeight - ScrollY);
                
                // 绘制行背景
                canvas.FillColor = row.RowColor;
                canvas.FillRectangle(0, y, dirtyRect.Width, (float)RowHeight);
                
                // 绘制单元格
                float x = 0;
                foreach (var cell in row.Cells)
                {
                    if (cell.Width <= 0) continue;
                    
                    // 设置字体
                    canvas.FontSize = (float)cell.FontSize;
                    if (Application.Current?.RequestedTheme == AppTheme.Dark)
                    {
                        canvas.FontColor = ValueTextColor.ContainsKey(cell.Value) ? ValueTextColor[cell.Value] : Colors.White;
                    }
                    else
                    {
                        canvas.FontColor = ValueTextColor.ContainsKey(cell.Value) ? ValueTextColor[cell.Value] : Colors.Black;
                    }

                    // 直接使用DrawString的垂直居中功能
                    float cellWidth = (float)cell.Width;
                    float cellHeight = (float)RowHeight;
                    float cellX = x + (float)cell.Padding;
                    float cellY = y + (float)cell.Padding;
                    
                    // 绘制文本 - 使用垂直和水平居中对齐
                    canvas.DrawString(
                        cell.Value, 
                        cellX, 
                        cellY, 
                        cellWidth - (float)cell.Padding * 2, 
                        cellHeight - (float)cell.Padding * 2, 
                        HorizontalAlignment.Center, 
                        VerticalAlignment.Center
                    );
                    
                    // 绘制列分隔线
                    canvas.StrokeColor = Colors.LightGray;
                    canvas.StrokeSize = 1;
                    canvas.DrawLine(x + cellWidth, y, x + cellWidth, y + (float)RowHeight);
                    
                    // 移动到下一个单元格
                    x += cellWidth;
                }
                
                // 绘制行分隔线
                canvas.StrokeColor = Colors.LightGray;
                canvas.StrokeSize = 1;
                canvas.DrawLine(0, y + (float)RowHeight, dirtyRect.Width, y + (float)RowHeight);
            }
        }
    }
} 