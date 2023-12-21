using System.Windows;
using System.Windows.Controls;

namespace MySharpChat.Client.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UserControl currentUC;
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
            WindowGrid.Children.Remove(currentUC);
            currentUC = connectionUC;
            WindowGrid.Children.Add(currentUC);
        }

        private void OnConnectionSucess()
        {
            WindowGrid.Children.Remove(currentUC);
            currentUC = chatUC;
            WindowGrid.Children.Add(currentUC);
        }
    }
}
