using System;
using MySharpChat.Client.Command;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MySharpChat.Client.GUI.Commands
{
    internal class WpfConnectCommand : ICommand
    {
        private bool _isExecuting = false;

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting;
        }

        public event EventHandler? CanExecuteChanged;

        public void Execute(object? parameter)
        {
            if (parameter is null)
                throw new ArgumentNullException(nameof(parameter));

            if (parameter is WpfConnectionArgs connectionArgs)
            {
                _isExecuting = true;
                OnCanExecuteChanged();
                try
                {
                    Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;
                    Task.Run(() =>
                    {
                        bool connectionSuccess = ConnectCommand.Instance.Execute(connectionArgs.ViewModel.Client, connectionArgs.ViewModel.ServerIp);
                        if (connectionSuccess)
                            currentDispatcher.Invoke(connectionArgs.ViewModel.OnConnectionSuccess);
                        else
                            currentDispatcher.Invoke(connectionArgs.ViewModel.OnConnectionFail);
                    });
                }
                finally
                {
                    _isExecuting = false;
                    OnCanExecuteChanged();
                }
            }
            else
            {
                throw new ArgumentException(string.Format("{0} must be of type {1}", nameof(parameter), typeof(WpfConnectionArgs)));
            }
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
