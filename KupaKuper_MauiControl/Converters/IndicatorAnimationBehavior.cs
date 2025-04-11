using System.ComponentModel;
using System.Diagnostics;

namespace KupaKuper_MauiControl.Converters
{
    /// <summary>
    /// 指示器动画行为：处理滑动选择器的动画效果
    /// </summary>
    public class IndicatorAnimationBehavior : Behavior<Border>
    {
        /// <summary>
        /// 上一次的位置
        /// </summary>
        private int _currentTranslation;
        /// <summary>
        /// 当行为附加到控件时调用
        /// </summary>
        protected override void OnAttachedTo(Border bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.PropertyChanged += OnBorderPropertyChanged;
            
            // 初始化位置
            MainThread.BeginInvokeOnMainThread(() => 
            {
                if (bindable?.Parent is Grid parentGrid)
                {
                    var column = Grid.GetColumn(bindable);
                    var width = parentGrid.Width / 3;
                    bindable.TranslationX = column * width;
                }
            });
        }

        /// <summary>
        /// 当行为从控件分离时调用
        /// </summary>
        protected override void OnDetachingFrom(Border bindable)
        {
            bindable.PropertyChanged -= OnBorderPropertyChanged;
            base.OnDetachingFrom(bindable);
        }

        /// <summary>
        /// 处理边框属性变化事件，实现滑动动画
        /// </summary>
        private void OnBorderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Column" && sender is Border border)
            {
                if (border?.Parent is Grid parentGrid)
                {
                    var parent = border.Parent as Grid;
                    var column = Grid.GetColumn(border);
                    var width = parentGrid.Width / parent.ColumnDefinitions.Count;
                    var start = (_currentTranslation - column) * width;
                    // 使用动画
                    var animation = new Animation(v =>border.TranslationX = v,
                        start,  // 开始位置
                        0,       // 结束位置
                        Easing.CubicInOut);   // 缓动函数
                    
                    animation.Commit(border, "SlideAnimation", 16, 150);
                    Debug.WriteLine("从" + start + "到" + 0);
                    _currentTranslation = column;
                }
            }
        }
    }
}
