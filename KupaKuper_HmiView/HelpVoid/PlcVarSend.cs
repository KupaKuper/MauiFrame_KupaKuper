using KupaKuper_IO.Ethernet;

using PlcClient = KupaKuper_IO.Ethernet.PlcClient;

namespace KupaKuper_HmiView.HelpVoid
{
    public static class PlcVarSend
    {
        /// <summary>
        /// 按下发送true到按钮绑定的地址
        /// </summary>
        /// <param name="sender"></param>
        public static void ButtonClicked_SetTrue(PlcVar sender)
        {
            if (!PlcClient.WriteClient.Connected) return;
            var var = sender;
            try
            {
                PlcClient.WriteClient.Write(var.PlcVarAddress, true);
            }
            catch (Exception ex)
            {
                ShowMessageErr(var.PlcVarName, ex.Message);
            }
        }

        /// <summary>
        /// 按下发送false到按钮绑定的地址
        /// </summary>
        /// <param name="sender"></param>
        public static void ButtonClicked_SetFalse(PlcVar sender)
        {
            if (!PlcClient.WriteClient.Connected) return;
            var var = sender;
            try
            {
                PlcClient.WriteClient.Write(var.PlcVarAddress, false);
            }
            catch (Exception ex)
            {
                ShowMessageErr(var.PlcVarName, ex.Message);
            }
        }

        /// <summary>
        /// 数值输入框发送输入值到PLC
        /// </summary>
        /// <param name="var"></param>
        /// <param name="value"></param>
        public static void NumberBos_SetValue(PlcVar var, string value)
        {
            if (!PlcClient.WriteClient.Connected) return;
            try
            {
                var v = Convert.ChangeType(value, var.PlcVarMode.GetType());
                PlcClient.WriteClient.Write(var.PlcVarAddress, v);
            }
            catch (Exception ex)
            {
                ShowMessageErr(var.PlcVarName, ex.Message);
            }
        }

        /// <summary>
        /// 弹窗显示错误信息
        /// </summary>
        /// <param name="VarName"></param>
        /// <param name="ErrMessage"></param>
        public static async void ShowMessageErr(string VarName, string ErrMessage)
        {
            string title = "写入出错";
            string message = "(" + VarName + ")" + "读写PLC数据出错" + ":  " + ErrMessage;
            await DisplayAlertHelp.TryDisplayAlert(title, message, "OK");
        }

    }
}
