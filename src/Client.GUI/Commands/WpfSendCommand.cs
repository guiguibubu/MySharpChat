using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using MySharpChat.Client.Command;

namespace MySharpChat.Client.GUI.Commands
{
    internal class WpfSendCommand : ICommand
    {
        private bool _isExecuting = false;

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting;
        }

        public event EventHandler? CanExecuteChanged;

        public void Execute(object? parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            if (parameter is WpfSendArgs sendArgs)
            {
                _isExecuting = true;
                OnCanExecuteChanged();
                try
                {
                    Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;
                    Task.Run(() =>
                    {
                        bool connectionSuccess = SendCommand.Instance.Execute(sendArgs.ViewModel.Client, sendArgs.ViewModel.InputMessage);
                        if (connectionSuccess)
                            currentDispatcher.Invoke(sendArgs.ViewModel.OnSendSuccess);
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
                throw new ArgumentException(string.Format("{0} must be of type {1}", nameof(parameter), typeof(WpfSendArgs)));
            }
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
