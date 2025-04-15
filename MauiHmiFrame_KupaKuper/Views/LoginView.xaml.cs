using KupaKuper_HmiView.Resources;

using MauiHmiFrame_KupaKuper.Resources;
using MauiHmiFrame_KupaKuper.ViewModes;

namespace MauiHmiFrame_KupaKuper.Views
{
    public partial class LoginView : ContentView
    {
        private readonly LogInVM _viewMode;
        public LoginView()
        {
            InitializeComponent();
            BindingContext = _viewMode = new();
        }
        public delegate void ChangeUser(int user);
        private string Password = "";
        /// <summary>
        /// �л��û�����ʱ����
        /// </summary>
        public event ChangeUser? UserChanged;

        private void LoginOp_Clicked(object sender, EventArgs e)
        {
            this.UserChanged?.Invoke(0);
        }

        private void LoginAdmin_Clicked(object sender, EventArgs e)
        {
            if (Password == _viewMode.Password)
            {
                this.UserChanged?.Invoke(1);
                _viewMode.TitlText = "";
            }
            else
            {
                _viewMode.TitlText = AppResources.ResourceManager?.GetString("PasswordErr") ?? "����Ա�����������";
            }
            PasswordBox.Text = null;
        }

        private void PasswordBox_Completed(object sender, EventArgs e)
        {
            var t = (Editor)sender;
            Password = t.Text;
        }
    }
}

