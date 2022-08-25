using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Threading;

namespace MySharpChat.Client.GUI.MAUI
{
    public partial class App : Application
    {
        public App() : base()
        {
            InitializeComponent();

            ClientImpl = new GuiClientImpl();

            clientMainThread = new Thread(StartClient);
            clientMainThread.Name = "ClientMainThread";

            MainWindowViewModel viewModel = new MainWindowViewModel(ClientImpl);
            mainWindow = new MainWindow(viewModel);

            Shell appShell = new AppShell(mainWindow);
            
            MainPage = appShell;
        }

        static App()
        {
            Logger.Factory.SetLoggingType(LoggerType.File);
        }

        private static readonly Logger logger = Logger.Factory.GetLogger<App>();

        internal GuiClientImpl ClientImpl { get; private set; }

        private readonly MainWindow mainWindow;
        private readonly Thread clientMainThread;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        protected override void OnStart()
        {
            base.OnStart();

            clientMainThread.Start(cancellationTokenSource.Token);
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            cancellationTokenSource.Cancel();
        }

        private void StartClient(object? obj)
        {
            if (obj is CancellationToken cancellationToken)
                StartClient(cancellationToken);
            else
                throw new ArgumentException(string.Format("{0} must be a {1}", nameof(obj), typeof(CancellationToken)));
        }

        private void StartClient(CancellationToken cancellationToken)
        {
            int exitCode;

            Client client = new Client(ClientImpl);

            try
            {
                if (client.Start())
                {
                    MainThread.BeginInvokeOnMainThread(() => MainPage = mainWindow);

                    int waitTimeMs = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;
                    while (!client.Wait(waitTimeMs))
                        cancellationToken.ThrowIfCancellationRequested();
                }

                exitCode = client.ExitCode;
                MainThread.BeginInvokeOnMainThread(() => Quit());
            }
            catch (OperationCanceledException)
            {
                client.Stop();
                exitCode = 0;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Program crash !");
                exitCode = 1;
                MainThread.BeginInvokeOnMainThread(() => Quit());
            }
        }
    }
}