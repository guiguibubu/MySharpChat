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
        public ChatUserControl()
        {
            InitializeComponent();

            OutputBox.TextWrapping = TextWrapping.Wrap;
            OutputBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            OutputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            InputBox.KeyDown += InputBox_KeyDown; 

            SendButton.Command = new SendCommand();
            SendButton.CommandParameter = this;
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            if (key == Key.Enter)
                Send();
        }

        public void Send()
        {
            string text = InputBox.Text;
            if (!string.IsNullOrEmpty(text))
            {
                OutputBox.AppendText(text + Environment.NewLine);
                OutputBoxScroller.ScrollToEnd();
                InputBox.Text = "";
                InputBox.Focus();
            }
        }
    }
}
