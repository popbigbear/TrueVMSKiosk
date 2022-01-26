using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        

        public void setMessage(string title, string msg1,string msg2)
        {
            lblAlertTitle.Content = title;
            lblAlertMessage.Content = msg1;
            lblAlertMessage2.Content = msg2;

            if(MainWindow.LANGUAGE == MainWindow.LANGUAGE_THAI)
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
