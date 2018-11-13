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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.ObjectModel;

namespace ChatUp
{
    public class ChatViewModel : BaseINPC
    {
        public ChatModel Chat { get; set; }
        

        public ChatViewModel()
        {
            Chat = new ChatModel();
            ConnectButtonCommand = new RelayCommand(new Action<object>(ConnectButtonClick));
            SendButtonCommand = new RelayCommand(new Action<object>(SendButtonClick));
        }
        public ICommand ConnectButtonCommand { get; set; }
        public ICommand SendButtonCommand { get; set; }

        private void ConnectButtonClick(object sender)
        {
            //implement button click here
            MessageBox.Show(sender.ToString());
        }

        private void SendButtonClick(object sender)
        {
            //implement send button click here
            MessageBox.Show(sender.ToString());
        }

        
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException("execute");

            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((object)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute((object)parameter);
        }

    }
}
