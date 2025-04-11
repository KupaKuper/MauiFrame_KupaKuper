using KupaKuper_HmiView.ContentViewModes;

namespace KupaKuper_HmiView.ContentViews
{
    public partial class AlarmView : ContentView
    {
        private readonly AlarmViewVM thisVM;
        public AlarmView()
        {
            InitializeComponent();
            BindingContext = thisVM = new AlarmViewVM();
            AlarmTable.TableDrawable.ValueTextColor.Add(AlarmType.Alarm.ToString(), Colors.Red);
            AlarmTable.TableDrawable.ValueTextColor.Add(AlarmType.Info.ToString(), Colors.Orange);
            AlarmTable.TableDrawable.ValueTextColor.Add("True", Colors.OrangeRed);
            AlarmTable.TableDrawable.ValueTextColor.Add("False", Colors.Brown);
            thisVM.SearchAlarmed += ThisVM_SearchAlarmed;
        }

        private void ThisVM_SearchAlarmed(string str)
        {
            AlarmTable.ViewModel.SearchText = str;
        }

        private void DatePicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            _ = thisVM.DateChanged(e.NewDate);
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.OldTextValue != "" && e.NewTextValue == "")
            {
                AlarmTable.ViewModel.SearchText = "";
            }
        }
    }
}

