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
    /// Interaction logic for ChatUserControl.xaml
    /// </summary>
    public partial class ChatUserControl : UserControl
    {
        private string SendTextValue => InputBox.Text;

        private readonly ChatViewModel m_viewModel;

        internal ChatUserControl(ChatViewModel viewModel)
        {
            InitializeComponent();

            m_viewModel = viewModel;

            OutputBox.TextWrapping = TextWrapping.Wrap;
            OutputBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            OutputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            InputBox.KeyDown += InputBox_KeyDown;

            SendButton.Command = new WpfSendCommand();
            SendButton.CommandParameter = new WpfSendArgs() { chatUC = this, client = (Application.Current as App)!.ClientImpl, args = new string[] { SendTextValue } }; ;
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            if (key == Key.Enter)
                SendButton.Command.Execute(SendButton.CommandParameter);
        }

        public void OnSendSuccess()
        {
            string text = SendTextValue;
            if (!string.IsNullOrEmpty(text))
            {
                OutputBox.AppendText((Application.Current as App)!.ClientImpl.Username + ": " + text + Environment.NewLine);
                OutputBoxScroller.ScrollToEnd();
                InputBox.Text = "";
                InputBox.Focus();
            }
        }
    }
}
