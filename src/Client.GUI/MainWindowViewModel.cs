namespace MySharpChat.Client.GUI
{
    internal class MainWindowViewModel
    {
        public GuiClientImpl Client { get; private set; }

        public MainWindowViewModel(GuiClientImpl client)
        {
            Client = client;
        }
    }
}
