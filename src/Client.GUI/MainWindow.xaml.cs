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
