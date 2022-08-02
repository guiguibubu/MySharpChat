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
        private readonly ChatViewModel m_viewModel;

        internal ChatUserControl(ChatViewModel viewModel)
        {
            InitializeComponent();

            m_viewModel = viewModel;

            InputBox.KeyDown += InputBox_KeyDown;
            InputBox.TextChanged += (object sender, TextChangedEventArgs e) => { m_viewModel.InputMessage = InputBox.Text; };
            SendButton.Command = new WpfSendCommand();
            SendButton.CommandParameter = new WpfSendArgs() { ViewModel = m_viewModel };

            m_viewModel.OnSendSuccessEvent += OnSendSuccessEvent;
        }

        private void OnSendSuccessEvent()
        {
            string text = m_viewModel.InputMessage;
            if (!string.IsNullOrEmpty(text))
            {
                TextBlock outpuBlock = new TextBlock();
                outpuBlock.TextWrapping = TextWrapping.Wrap;
                outpuBlock.Margin = new Thickness(0,2,0,2);
                outpuBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
                outpuBlock.VerticalAlignment = VerticalAlignment.Center;
                outpuBlock.Background = new SolidColorBrush(Colors.WhiteSmoke);
                outpuBlock.Text = m_viewModel.Client.Username + ": " + text;
                
                OutputStack.Children.Add(outpuBlock);
                OutputScroller.ScrollToEnd();

                InputBox.Text = "";
                InputBox.Focus();
            }
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            if (key == Key.Enter)
                SendButton.Command.Execute(SendButton.CommandParameter);
        }
    }
}
