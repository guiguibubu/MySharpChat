using MySharpChat.Core.Utils.Logger;
using System;
using System.Threading;
using System.Windows;

namespace MySharpChat.Client.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() : base()
        {
            
            ClientImpl = new GuiClientImpl();

            clientMainThread = new Thread(StartClient);
            clientMainThread.Name = "ClientMainThread";

            MainWindowViewModel viewModel = new MainWindowViewModel(ClientImpl);
            mainWindow = new MainWindow(viewModel);
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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            clientMainThread.Start(cancellationTokenSource.Token);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
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
                    Dispatcher.Invoke(() => mainWindow.Show());

                    int waitTimeMs = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;
                    while (!client.Wait(waitTimeMs))
                        cancellationToken.ThrowIfCancellationRequested();
                }

                exitCode = client.ExitCode;
                Dispatcher.Invoke(() => Shutdown(exitCode));
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
                Dispatcher.Invoke(() => Shutdown(exitCode));
            }
        }
    }
}
