using System;
using MySharpChat.Client.Command;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MySharpChat.Client.GUI.Commands
{
    internal class WpfDisconnectCommand : ICommand
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

            if (parameter is WpfDisconnectionArgs connectionArgs)
            {
                _isExecuting = true;
                OnCanExecuteChanged();
                try
                {
                    Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;
                    Task.Run(() =>
                    {
                        DisconnectCommand.Instance.Execute(connectionArgs.ViewModel.Client);
                        currentDispatcher.Invoke(connectionArgs.ViewModel.OnDisconnection, true);
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
                throw new ArgumentException(string.Format("{0} must be of type {1}", nameof(parameter), typeof(WpfDisconnectionArgs)));
            }
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
