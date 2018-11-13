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
        public const string ChatsPropertyName = "Chats";

        private ListBox _chatListBox;
        private ObservableCollection<ChatModel> _chats;

        public ObservableCollection<ChatModel> Chats
        {
            get
            {
                return _chats;
            }
            set
            {
                if (_chats == value)
                {
                    return;
                }
                _chats = value;
                RaisePropertyChanged("Chats");
            }
        }

        public ObservableCollection<ListBox> ListBoxes
        {
            get
            {
                return _listBoxes;
            }
            set
            {
                if (_listBoxes == value)
                {
                    return;
                }
                _listBoxes = value;
                RaisePropertyChanged("ListBoxes");
            }
        }

        public ChatViewModel()
        {
            _chatListBox = new ListBox();

            _chats = new ObservableCollection<ChatModel>();
            ConnectButtonCommand = new RelayCommand(new Action<object>(ConnectButtonClick));
            AddChatButtonCommand = new RelayCommand(new Action<object>(AddChatButtonClick));
        }
        private ICommand _connectButtonCommand;
        public ICommand ConnectButtonCommand
        {
            get
            {
                return _connectButtonCommand;
            }
            set
            {
                _connectButtonCommand = value;
            }
        }
        private ICommand _addChatButtonCommand;
        public ICommand AddChatButtonCommand
        {
            get
            {
                return _addChatButtonCommand;
            }
            set
            {
                _addChatButtonCommand = value;
            }
        }


        private void ConnectButtonClick(object sender)
        {
            //implement button click here
            MessageBox.Show(sender.ToString());
        }

        private void AddChatButtonClick(object sender)
        {
            //add new chat
            _chats.Add(new ChatModel() { Name = sender + "1" });
            //create new list box with 6 list box items
            ListBox newList = new ListBox();
            newList.Background = System.Windows.Media.Brushes.Transparent;
            newList.Foreground = System.Windows.Media.Brushes.White;
            newList.Items.Add(new ListBoxItem
            {
                Content = " "
            });
            newList.Items.Add(new ListBoxItem
            {
                Content = "{binding LocalIp}"
            });
            newList.Items.Add(new ListBoxItem
            {
                Content = "{binding LocalPort}"
            });
            newList.Items.Add(new ListBoxItem
            {
                Content = " "
            });
            newList.Items.Add(new ListBoxItem
            {
                Content = "{binding RemoteIp}"
            });
            newList.Items.Add(new ListBoxItem
            {
                Content = "{binding RemotePort}"
            });
                        
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
