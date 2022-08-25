using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Windows;

namespace MySharpChat.Client.GUI.MAUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ContentPage
    {
        private ContentView currentUC;
        private readonly ChatUserControl chatUC;
        private readonly ConnectionUserControl connectionUC;
        private readonly MainWindowViewModel m_viewModel;

        internal MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            m_viewModel = viewModel;

            ConnectionViewModel connectionViewModel = new ConnectionViewModel(m_viewModel.Client);
            connectionViewModel.OnConnectionSuccessEvent += OnConnectionSucess;

            connectionUC = new ConnectionUserControl(connectionViewModel);

            ChatViewModel chatViewModel = new ChatViewModel(m_viewModel.Client);
            chatUC = new ChatUserControl(chatViewModel);
            chatUC.OnDisconnectionEvent += OnDisconnection;
            chatUC.OnDisconnectionEvent += connectionViewModel.OnDisconnection;
            currentUC = connectionUC;

            WindowGrid.Children.Add(currentUC);
        }

        private void OnDisconnection(bool manual)
        {
            if (MainThread.IsMainThread)
            {
                WindowGrid.Children.Remove(currentUC);
                currentUC = connectionUC;
                WindowGrid.Children.Add(currentUC);
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
                WindowGrid.Children.Remove(currentUC);
                currentUC = chatUC;
                WindowGrid.Children.Add(currentUC);
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() => OnConnectionSucess());
            }
        }
    }
}
