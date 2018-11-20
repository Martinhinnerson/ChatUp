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
using System.IO;

namespace ChatApp
{
    public class ChatViewModel : BaseViewModel
    {
        public ChatModel Chat { get; set; }

        private string _inviteButtonLabel;
        public string InviteButtonLabel
        {
            get { return _inviteButtonLabel; }
            set
            {
                _inviteButtonLabel = value;
                RaisePropertyChanged("InviteButtonLabel");
            }
        }

        private string _listenButtonLabel;
        public string ListenButtonLabel
        {
            get { return _listenButtonLabel; }
            set
            {
                _listenButtonLabel = value;
                RaisePropertyChanged("ListenButtonLabel");
            }
        }

        private string _sendText;
        public string SendText
        {
            get { return _sendText; }
            set
            {
                _sendText = value;
                RaisePropertyChanged("SendText");
            }
        }

        public ChatViewModel()
        {
            ListenButtonLabel = "Search for connection";
            InviteButtonLabel = "Invite";
            Chat = new ChatModel();
            ListenButtonCommand = new RelayCommand(new Action<object>(ListenButtonClick));
            InviteButtonCommand = new RelayCommand(new Action<object>(InviteButtonClick));
            SendButtonCommand = new RelayCommand(new Action<object>(SendButtonClick));
            AcceptButtonCommand = new RelayCommand(new Action<object>(AcceptButtonClick));
            DeclineButtonCommand = new RelayCommand(new Action<object>(DeclineButtonClick));
            DisconnectClientCommand = new RelayCommand(new Action<object>(DisconnectClientClick));

            Chat.AddressBusy += AddressAlreadyInUse; //subscribe to event for receiving messages

        }
        public ICommand InviteButtonCommand { get; set; }
        public ICommand ListenButtonCommand { get; set; }
        public ICommand SendButtonCommand { get; set; }
        public ICommand AcceptButtonCommand { get; set; }
        public ICommand DeclineButtonCommand { get; set; }
        public ICommand DisconnectClientCommand { get; set; }

        private void ListenButtonClick(object sender)
        {
            if (!Chat.IsNotListening)
            {
                ListenButtonLabel = "Search for connection";
                Chat.StopListening();
            }
            else
            {
                ListenButtonLabel = "Stop searching";
                Chat.StartListening();
            }
        }
        private void InviteButtonClick(object sender)
        {
            Chat.Invite();
        }


        private void SendButtonClick(object sender)
        {
            if (!Chat.Clients.Any()) //if there are no clients in the client list
            {
                Chat.Status = "There are no clients connected";
                Console.WriteLine("There are no clients connected");
            }
            else
            {
                //Send to selected client
                Chat.SendToSelectedClient(SendText);
                SendText = "";
            }
        }

        private void AcceptButtonClick(object sender)
        {
            Chat.Accept = true;
            Chat.ShowPopup = false;
        }
        private void DeclineButtonClick(object sender)
        {
            Chat.Accept = false;
            Chat.ShowPopup = false;
        }

        private void DisconnectClientClick(object sender)
        {
            Message msg = new Message(Chat.UserName);
            Chat.Clients.Single(i => i.Name == Chat.SelectedClient.Name).SendString(msg.GetDisconnectMessage());//Look for exception here
            Chat.DisconnectSelectedClient();
        }

        private void AddressAlreadyInUse()
        {
            MessageBox.Show("Address is busy.\nTry to send and invite instead.");
            ListenButtonLabel = "Search for connection";
            Chat.StopListening();
        }
    }
}
