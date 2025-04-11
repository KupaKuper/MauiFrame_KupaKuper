
using KupaKuper_IO.Ethernet;

namespace KupaKuper_MauiControl.Controls
{
    public class HmiButton : Button
    {
        public HmiButton()
        {
            this.Pressed += HmiButton_Pressed;
            this.Released += HmiButton_Released;
        }

        async void HmiButton_Released(object? sender, EventArgs e)
        {
            await this.ScaleTo(1, 10);
        }

        async void HmiButton_Pressed(object? sender, EventArgs e)
        {
            await this.ScaleTo(0.92, 15);
        }
        /// <summary>
        /// PLC变量的绑定属性
        /// </summary>
        public static readonly BindableProperty VarProperty = BindableProperty.Create(
            nameof(Var),
            typeof(PlcVar),
            typeof(HmiButton),
            null);

        /// <summary>
        /// 获取或设置PLC变量
        /// </summary>
        public PlcVar Var
        {
            get => (PlcVar)GetValue(VarProperty);
            set => SetValue(VarProperty, value);
        }
    }
}

