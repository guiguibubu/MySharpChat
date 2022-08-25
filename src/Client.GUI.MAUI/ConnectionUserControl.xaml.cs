using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MySharpChat.Client.GUI.MAUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MySharpChat.Client.GUI.MAUI
{
    /// <summary>
    /// Interaction logic for ConnectionUserControl.xaml
    /// </summary>
    public partial class ConnectionUserControl : ContentView
    {
        private readonly ConnectionViewModel m_viewModel;
        internal ConnectionUserControl(ConnectionViewModel viewModel)
        {
            InitializeComponent();

            m_viewModel = viewModel;

            UserNameInputBox.TextChanged += (object? sender, TextChangedEventArgs e) => { m_viewModel.Client.SetUsername(e.NewTextValue); };
            IpInputBox.TextChanged += (object? sender, TextChangedEventArgs e) => { m_viewModel.ServerIp = e.NewTextValue; };
            PortInputBox.TextChanged += (object? sender, TextChangedEventArgs e) => { m_viewModel.ServerPort = e.NewTextValue; };

            ConnectButton.Command = new WpfConnectCommand();
            ConnectButton.CommandParameter = new WpfConnectionArgs() { ViewModel = m_viewModel };
            ConnectButton.Clicked += ConnectButton_Click;

            m_viewModel.OnConnectionSuccessEvent += OnConnectionSucess;
            m_viewModel.OnConnectionFailEvent += OnConnectionFail;
            m_viewModel.OnDisconnectionEvent += OnDisconnection;
        }

        private void ConnectButton_Click(object? sender, EventArgs e)
        {
            ConnectButton.IsEnabled = false;
        }

        private void OnDisconnection(bool manual)
        {
            if (MainThread.IsMainThread)
            {
                ConnectionStatus.Text = manual ? "Disconnect successfully !" : "Connection lost !";
                ConnectionStatus.TextColor = manual ? Colors.LimeGreen : Colors.Red;
                ConnectButton.IsEnabled = true;
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() => OnDisconnection(manual));
            }
        }

        private void OnConnectionSucess()
        {
            if (MainThread.IsMainThread)
            {
                ConnectionStatus.Text = "Connection success !";
                ConnectionStatus.TextColor = Colors.LimeGreen;
                ConnectButton.IsEnabled = true;
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() => OnConnectionSucess());
            }
        }

        private void OnConnectionFail()
        {
            if (MainThread.IsMainThread)
            {
                ConnectionStatus.Text = "Connection failed !";
                ConnectionStatus.TextColor = Colors.Red;
                ConnectButton.IsEnabled = true;
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() => OnConnectionFail());
            }
        }
    }
}
