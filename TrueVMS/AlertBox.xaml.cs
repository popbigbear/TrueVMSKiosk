using System.Windows;
using System.Windows.Input;

namespace TrueVMS
{
    /// <summary>
    /// Interaction logic for AlertBox.xaml
    /// </summary>
    public partial class AlertBox : Window
    {

        public AlertBox()
        {
            InitializeComponent();
        }



        public void setMessage(string title, string msg1, string msg2, string msg3 = "")
        {
            lblAlertTitle.Content = title;
            lblAlertMessage.Content = msg1;
            lblAlertMessage2.Content = msg2;
            try
            {
                grid4.Height = new GridLength(0);
                lblAlertMessage3.Content = "";
                if (msg3 != null)
                {
                    if (msg3.Trim() != "")
                    {
                        grid4.Height = new GridLength(60);
                        lblAlertMessage3.Content = msg3;
                    }
                }
            }
            catch { }

            if (MainWindow.LANGUAGE == MainWindow.LANGUAGE_THAI)
            {
                btnClose.Content = "ปิด";
            }
            else
            {
                btnClose.Content = "Close";
            }
        }

        private void btnClose_TouchDown(object sender, TouchEventArgs e)
        {
            this.Hide();
        }
    }
}
