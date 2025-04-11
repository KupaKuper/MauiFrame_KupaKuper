using CommunityToolkit.Mvvm.ComponentModel;

namespace KupaKuper_MauiControl.ControlModes
{
    public partial class SwitchableLableViewMode : ObservableObject
    {
        /// <summary>
        /// 标签名称列表
        /// </summary>
        private List<string> _listViewName = new() { "默认页面" };
        public List<string> ListViewName
        {
            get
            {
                return _listViewName;
            }
            set
            {
                if (value != _listViewName)
                {
                    _listViewName = value;
                    OnPropertyChanged(nameof(ListViewName));
                }
            }
        }
        /// <summary>
        /// 标签栏的高度
        /// </summary>
        [ObservableProperty]
        private float lableHeight = 40;

        public void ChangeLableList(List<string> Names)
        {
            ListViewName = Names;
        }
    }
}