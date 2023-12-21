using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MySharpChat.Client.GUI.Commands;

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
            m_viewModel.OnDisconnectionEvent += OnDisconnection;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectButton.IsEnabled = false;
        }

        private void OnDisconnection(bool manual)
        {
            ConnectionStatus.Text = manual ? "Disconnect successfully !" : "Connection lost !";
            ConnectionStatus.Foreground = manual ? new SolidColorBrush(Colors.LimeGreen) : new SolidColorBrush(Colors.Red);
            ConnectButton.IsEnabled = true;
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
