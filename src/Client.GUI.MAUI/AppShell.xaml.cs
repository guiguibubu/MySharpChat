using Microsoft.Maui.Controls;

namespace MySharpChat.Client.GUI.MAUI
{
    public partial class AppShell : Shell
    {
        public AppShell(MainWindow mainWindow) : base()
        {
            InitializeComponent();

            MyShellContent.Title = "Home";
            MyShellContent.ContentTemplate = new DataTemplate(() => mainWindow);
        }
    }
}