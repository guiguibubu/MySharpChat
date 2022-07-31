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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MySharpChat.Client.GUI
{
    /// <summary>
    /// Interaction logic for ConnectionUserControl.xaml
    /// </summary>
    public partial class ConnectionUserControl : UserControl
    {
        public ConnectionUserControl()
        {
            InitializeComponent();

            ConnectButton.Command = new ConnectCommand();
            ConnectButton.CommandParameter = this;
        }

        public event Action OnConnectionSuccessEvent = () => { };
        public void OnConnectionSuccess()
        {
            IsEnabled = !IsEnabled;
            Visibility = (Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;
            OnConnectionSuccessEvent();
        }
    }
}
