using MySharpChat.Client.GUI.Commands;
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
        private readonly ConnectionViewModel m_viewModel;
        internal ConnectionUserControl(ConnectionViewModel viewModel)
        {
            InitializeComponent();

            m_viewModel = viewModel;

            UserNameInputBox.TextChanged += (object sender, TextChangedEventArgs e) => { m_viewModel.Client.SetUsername(UserNameInputBox.Text); };
            IpInputBox.TextChanged += (object sender, TextChangedEventArgs e) => { m_viewModel.ServerIp = IpInputBox.Text; };
            PortInputBox.TextChanged += (object sender, TextChangedEventArgs e) => { m_viewModel.ServerPort = PortInputBox.Text; };

            ConnectButton.Command = new WpfConnectCommand();
            ConnectButton.CommandParameter = new WpfConnectionArgs() { ViewModel = m_viewModel };
            ConnectButton.Click += ConnectButton_Click;

            m_viewModel.OnConnectionSuccessEvent += OnConnectionSucess;
            m_viewModel.OnConnectionFailEvent += OnConnectionFail;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectButton.IsEnabled = false;
        }

        private void OnConnectionSucess()
        {
            ConnectionStatus.Text = "Connection success !";
            ConnectionStatus.Foreground = new SolidColorBrush(Colors.LimeGreen);
            ConnectButton.IsEnabled = true;
        }

        private void OnConnectionFail()
        {
            ConnectionStatus.Text = "Connection failed !";
            ConnectionStatus.Foreground = new SolidColorBrush(Colors.Red);
            ConnectButton.IsEnabled = true;
        }
    }
}
