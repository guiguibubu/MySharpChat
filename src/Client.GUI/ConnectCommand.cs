using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MySharpChat.Client.GUI
{
    internal class ConnectCommand : ICommand
    {
        private bool _isExecuting = false;

        public bool CanExecute(object parameter)
        {
            return !(_isExecuting);
        }

        public event EventHandler CanExecuteChanged;

        public async void Execute(object parameter)
        {
            if(parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            if(parameter is ConnectionUserControl connectionUC)
            {
                _isExecuting = true;
                OnCanExecuteChanged();
                try
                {
                    connectionUC.OnConnectionSuccess();
                }
                finally
                {
                    _isExecuting = false;
                    OnCanExecuteChanged();
                }
            }
            else
            {
                throw new ArgumentException(string.Format("{0} must be of type {1}", nameof(parameter), typeof(ConnectionUserControl)));
            }
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
