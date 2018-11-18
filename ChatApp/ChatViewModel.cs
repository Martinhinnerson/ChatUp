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
            ListenButtonLabel = "Listen Local";
            InviteButtonLabel = "Invite Remote";
            Chat = new ChatModel();
            ListenButtonCommand = new RelayCommand(new Action<object>(ListenButtonClick));
            InviteButtonCommand = new RelayCommand(new Action<object>(InviteButtonClick));
            SendButtonCommand = new RelayCommand(new Action<object>(SendButtonClick));
            AcceptButtonCommand = new RelayCommand(new Action<object>(AcceptButtonClick));
            DeclineButtonCommand = new RelayCommand(new Action<object>(DeclineButtonClick));

            Chat.ReceivedMessageFromClient += ReceivedMessageFromClient; //subscribe to event for receiving messages

        }
        public ICommand InviteButtonCommand { get; set; }
        public ICommand ListenButtonCommand { get; set; }
        public ICommand SendButtonCommand { get; set; }
        public ICommand AcceptButtonCommand { get; set; }
        public ICommand DeclineButtonCommand { get; set; }

        private void ListenButtonClick(object sender)
        {
            if (Chat.IsListening)
            {
                ListenButtonLabel = "Listen Local";
                Chat.StopListening();
            }
            else
            {
                ListenButtonLabel = "Disconnect";
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
                Console.WriteLine("There are no clients to send to.");
            }
            else
            {
                //Right now we send to all clients and not only the selected one
                Chat.Clients.ForEach(client => client.SendString(Chat.UserName + SendText));
                //SendText = "";
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

        private static void ReceivedMessageFromClient(object sender, string str)
        {
            Console.WriteLine("Received message from client {0}: {1}", (sender as Client).ID, str);
        }
        
    }
}
