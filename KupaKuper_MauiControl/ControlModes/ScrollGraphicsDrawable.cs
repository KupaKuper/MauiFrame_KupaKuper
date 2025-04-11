using System.Collections.ObjectModel;

namespace KupaKuper_MauiControl.ControlModes
{
    public class ScrollGraphicsDrawable : IDrawable
    {
        public enum CalculateLayoutType
        {
            Width,
            Margin
        }
        #region 必要属性
        /// <summary>
        /// 项目集合
        /// </summary>
        public required ObservableCollection<object>? Items { get; set; }
        /// <summary>
        /// 视口宽度
        /// </summary>
        public required double ViewportWidth { get; set; }
        /// <summary>
        /// 视口高度
        /// </summary>
        public required double ViewportHeight { get; set; }
        /// <summary>
        /// 项目宽度
        /// </summary>
        public required float ItemWidth { get; set; }
        /// <summary>
        /// 项目高度
        /// </summary>
        public required float ItemHeight { get; set; }
        /// <summary>
        /// 绘制单独一项的布局,使用IsButtonPressed获取项按钮状态,使用AddButtonRectF在绘制按钮后更新按钮位置
        /// </summary>
        public event delDraw? DrawItem;
        /// <summary>
        /// 绘制前的准备方法,不用可以为空
        /// </summary>
        public event BeforeDraw? BeforeDrawItem;
        #endregion
        #region 属性
        /// <summary>
        /// 滚动位置
        /// </summary>
        public double ScrollY { get; set; } = 0;
        /// <summary>
        /// 项目横向间隔
        /// </summary>
        public float ItemMarginX { get; set; } = 10;
        /// <summary>
        /// 项目竖向间隔
        /// </summary>
        public float ItemMarginY { get; set; } = 10;
        /// <summary>
        /// 排列的列数
        /// </summary>
        public int Columns { get;private set; } = 4;
        /// <summary>
        /// 输出的计算数据
        /// </summary>
        public string Message { get; private set; } = "";
        /// <summary>
        /// 布局计算时采用更改间距还是更改宽度
        /// </summary>
        public CalculateLayoutType CalculateLayout { get; set; } = CalculateLayoutType.Margin;
        #endregion
        #region 局部变量
        /// <summary>
        /// 按钮区域字典，用于点击检测
        /// </summary>
        private Dictionary<int, List<RectF>> _buttonRects = new();
        /// <summary>
        /// 添加按钮状态跟踪
        /// </summary>
        private Dictionary<string, bool> _buttonPressedState = new();
        /// <summary>
        /// 格式: pressed_itemIndex_buttonIndex
        /// </summary>
        private const string ButtonPressedKey = "pressed_{0}_{1}"; 
        /// <summary>
        /// 项目位置列表
        /// </summary>
        private List<PointF> _itemPositions = new List<PointF>();
        #endregion
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            try
            {
                // 清空画布
                canvas.FillColor = Colors.Transparent;
                canvas.FillRectangle(dirtyRect);

                // 重置按钮区域字典
                _buttonRects = new();

                // 如果没有数据，返回
                if (Items == null || Items.Count == 0)
                    return;
                
                // 调用绘制前准备
                BeforeDrawItem?.Invoke();

                // 重新计算布局以适应当前视口
                switch(CalculateLayout)
                {
                    case CalculateLayoutType.Margin:CalculateLayout_Margin(ViewportWidth);break;
                    case CalculateLayoutType.Width:CalculateLayout_Width(ViewportWidth);break;
                }
                
                // 计算项目位置
                CalculateItemPositions();
                
                // 绘制每个项目
                for (int i = 0; i < Items.Count; i++)
                {
                    if (i < _itemPositions.Count)
                    {
                        var pos = _itemPositions[i];
                        if (IsItemVisible(pos.Y, ScrollY))
                        {
                            DrawItem?.Invoke(canvas, pos.X, pos.Y - (float)ScrollY, ItemWidth, ItemHeight, Items[i], i);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Message += ($"气缸绘制错误: {ex.Message}") + "\r\n";
            }
        }
        /// <summary>
        /// 根据视口宽度计算布局_修改间距
        /// </summary>
        /// <param name="viewportWidth"></param>
        private void CalculateLayout_Margin(double viewportWidth)
        {
            ViewportWidth = viewportWidth;

            // 计算最小间距宽度（保证可读性）
            double minItemMargin = 10;

            // 计算最大可能的列数
            int maxColumns = Math.Max(1, (int)(viewportWidth / (ItemWidth + minItemMargin * 2)));

            // 设置列数
            Columns = maxColumns;

            // 计算每个气缸项的实际间距（均匀分布）
            double availableMargin = viewportWidth - (ItemWidth * Columns);
            ItemMarginX = (float)Math.Floor(availableMargin / (Columns * 2));

            // 确保项目间距不小于最小间距
            if (ItemMarginX < minItemMargin)
            {
                Columns = Math.Max(1, (int)(viewportWidth / (ItemWidth + minItemMargin * 2)));
                availableMargin = viewportWidth - (ItemWidth * Columns);
                ItemMarginX = (float)Math.Floor(availableMargin / (Columns * 2));
            }
        }
        private float minWidth;
        /// <summary>
        /// 根据视口宽度计算布局_修改宽度
        /// </summary>
        /// <param name="viewportWidth"></param>
        private void CalculateLayout_Width(double viewportWidth)
        {
            if (minWidth == 0) minWidth = ItemWidth;
            ViewportWidth = viewportWidth;

            // 计算最小项宽度（保证可读性）
            double minItemWidth = minWidth;

            // 计算最大可能的列数
            int maxColumns = Math.Max(1, (int)(ViewportWidth / (minItemWidth + ItemMarginX * 2)));

            // 设置列数
            Columns = maxColumns;

            // 计算每个项的实际宽度（均匀分布）
            double availableWidth = ViewportWidth - (ItemMarginX * 2 * Columns);
            ItemWidth = (float)Math.Floor(availableWidth / Columns);

            // 确保项宽度不小于最小宽度
            if (ItemWidth < minItemWidth)
            {
                Columns = Math.Max(1, (int)(ViewportWidth / (minItemWidth + ItemMarginX * 2)));
                availableWidth = ViewportWidth - (ItemMarginX * 2 * Columns);
                ItemWidth = (float)Math.Floor(availableWidth / Columns);
            }
        }
        /// <summary>
        /// 点击位置计算
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public (int cylinderIndex, int buttonIndex) HitTest(Point point)
        {
            try
            {
                // 直接检查点击位置是否在任何按钮区域内
                foreach (var entry in _buttonRects)
                {
                    int cylinderIndex = entry.Key;
                    var buttonRects = entry.Value;

                    for (int buttonIndex = 0; buttonIndex < buttonRects.Count; buttonIndex++)
                    {
                        // 获取按钮区域并考虑滚动位置
                        var rect = buttonRects[buttonIndex];

                        // 创建考虑滚动位置的点击点
                        var adjustedPoint = new PointF((float)point.X, (float)point.Y);

                        if (rect.Contains(adjustedPoint))
                        {
                            return (cylinderIndex, buttonIndex);
                        }
                    }
                }
                return (-1, -1); // 没有点击到任何按钮
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"点击测试错误: {ex.Message}");
                return (-1, -1);
            }
        }
        /// <summary>
        /// 设置按钮按下状态
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="buttonIndex"></param>
        /// <param name="isPressed"></param>
        public void SetButtonPressed(int itemIndex, int buttonIndex, bool isPressed)
        {
            string key = string.Format(ButtonPressedKey, itemIndex, buttonIndex);
            _buttonPressedState[key] = isPressed;
        }
        /// <summary>
        /// 检查按钮是否处于按下状态
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="buttonIndex"></param>
        /// <returns></returns>
        public bool IsButtonPressed(int itemIndex, int buttonIndex)
        {
            string key = string.Format(ButtonPressedKey, itemIndex, buttonIndex);
            return _buttonPressedState.TryGetValue(key, out bool pressed) && pressed;
        }
        /// <summary>
        /// 存储按钮区域，用于点击检测
        /// </summary>
        /// <param name="itemIndex"> 所属数据项序号 </param>
        /// <param name="buttonsRectF"> 按钮位置 </param>
        public void AddButtonRectF(int itemIndex,List<RectF> buttonsRectF)
        {
            if (!_buttonRects.ContainsKey(itemIndex))
            {
                _buttonRects[itemIndex] = new List<RectF>();
                foreach (var button in buttonsRectF)
                {
                    _buttonRects[itemIndex].Add(new RectF());
                }
            }

            // 更新按钮区域，考虑实际屏幕位置
            for (int i = 0; i < buttonsRectF.Count; i++)
            {
                _buttonRects[itemIndex][i] = buttonsRectF[i];
            }
        }
        /// <summary>
        /// 更新布局
        /// </summary>
        /// <param name="viewportWidth"></param>
        public void UpdateLayout(float viewportWidth)
        {
            ViewportWidth = viewportWidth;
            
            // 确保使用正确的项目尺寸
            if (ItemWidth <= 0)
                ItemWidth = 200; // 设置默认值
            
            if (ItemHeight <= 0)
                ItemHeight = 100; // 设置默认值
            
            // 重新计算布局以适应当前视口
            switch (CalculateLayout)
            {
                case CalculateLayoutType.Margin: CalculateLayout_Margin(viewportWidth); break;
                case CalculateLayoutType.Width: CalculateLayout_Width(viewportWidth); break;
            }

            // 计算项目位置
            CalculateItemPositions();
        }
        /// <summary>
        /// 绘制单独一项的布局,使用IsButtonPressed获取项按钮状态,使用AddButtonRectF在绘制按钮后更新按钮位置
        /// </summary>
        /// <param name="canvas"> 绘制实现的接口 </param>
        /// <param name="x"> 绘制的起点x </param>
        /// <param name="y"> 绘制的起点y </param>
        /// <param name="item"> 绘制的项 </param>
        /// <param name="index"> 绘制的第几项数据 </param>
        public delegate void delDraw(ICanvas canvas, float x, float y, float Width, float Height, object item, int index);
        /// <summary>
        /// 绘制前的准备方法
        /// </summary>
        public delegate void BeforeDraw();
        /// <summary>
        /// 计算项目位置
        /// </summary>
        protected void CalculateItemPositions()
        {
            _itemPositions.Clear();

            if (Items == null || Items.Count == 0)
                return;

            // 计算每项的位置
            int row = 0;
            int col = 0;
            
            for (int i = 0; i < Items.Count; i++)
            {
                // 计算项目的X和Y坐标
                float x = ItemMarginX + col * (ItemWidth + ItemMarginX * 2);
                float y = ItemMarginY + row * (ItemHeight + ItemMarginY * 2);
                
                // 添加到位置列表
                _itemPositions.Add(new PointF(x, y));
                
                // 更新行列
                col++;
                if (col >= Columns)
                {
                    col = 0;
                    row++;
                }
            }
            
            // 计算总内容高度
            float totalContentHeight = (float)Math.Ceiling((double)Items.Count / Columns) * (ItemHeight + ItemMarginY * 2);
        }
        /// <summary>
        /// 检查项目是否在可视区域内
        /// </summary>
        /// <param name="itemY"> 项目Y坐标 </param>
        /// <param name="scrollY"> 滚动位置 </param>
        /// <returns></returns>
        protected bool IsItemVisible(float itemY, double scrollY)
        {
            // 计算项目相对于视口的位置
            float relativeY = itemY - (float)scrollY;
            
            // 判断项目是否在视口内
            return relativeY + ItemHeight > 0 && relativeY < ViewportHeight;
        }
    }
}
