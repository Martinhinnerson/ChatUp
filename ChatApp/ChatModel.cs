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
using System.Windows.Threading;

namespace ChatApp
{
    public class ChatModel : BaseViewModel
    {

        private int _localPort;
        public int LocalPort
        {
            get { return _localPort; }
            set
            {
                _localPort = value;
                RaisePropertyChanged("LocalPort");
            }
        }

        private int _remotePort;
        public int RemotePort
        {
            get { return _remotePort; }
            set
            {
                _remotePort = value;
                RaisePropertyChanged("RemotePort");
            }
        }

        private string _localIP;
        public string LocalIP
        {
            get { return _localIP; }
            set
            {
                _localIP = value;
                RaisePropertyChanged("LocalIP");
            }
        }

        private string _remoteIP;
        public string RemoteIP
        {
            get { return _remoteIP; }
            set
            {
                _remoteIP = value;
                RaisePropertyChanged("RemoteIP");
            }
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                RaisePropertyChanged("UserName");
            }
        }
        
        private int _numClients; // Store how many clients is connected

        private bool _isNotListening;
        public bool IsNotListening
        {
            get { return _isNotListening; }
            set
            {
                _isNotListening = value;
                RaisePropertyChanged("IsNotListening");
            }
        } 
        
        public bool IsConnected { get; set; } //not used...

        private ObservableCollection<Client> _clients;//Store all connected clients
        public ObservableCollection<Client> Clients
        {
            get { return _clients; }
            set
            {
                _clients = value;
                RaisePropertyChanged("Clients");
            }
        }

        private Client _selectedClient; //Which client are we looking at in the chat window
        public Client SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                _selectedClient = value;
                RaisePropertyChanged("SelectedClient");
            }
        }

        private Client _newClient; 
        public Client NewClient
        {
            get { return _newClient; }
            set
            {
                _newClient = value;
                RaisePropertyChanged("NewClient");
            }
        }

        private string _lastReceivedMessage;
        public string LastReceivedMessage
        {
            get { return _lastReceivedMessage; }
            set
            {
                _lastReceivedMessage = value;
                RaisePropertyChanged("LastReceivedMessage");
            }
        }

        public delegate void AddresAlreadyInUse();
        public event AddresAlreadyInUse AddressBusy;

        public bool Accept { get; set; }

        //public bool WaitForName { get; set; }

        public AutoResetEvent WaitForUser = new AutoResetEvent(false);
        public bool InviteAccepted { get; set; }

        private bool _showPopup;
        public bool ShowPopup
        {
            get { return _showPopup; }
            set
            {
                _showPopup = value;
                RaisePropertyChanged("ShowPopup");
            }
        }

        private string _popupMessage;
        public string PopupMessage
        {
            get { return _popupMessage; }
            set
            {
                _popupMessage = value;
                RaisePropertyChanged("PopupMessage");
            }
        }

        private string _newConnectionName;
        public string NewConnectionName
        {
            get { return _newConnectionName; }
            set
            {
                _newConnectionName = value;
                RaisePropertyChanged("NewConnectionName");
            }
        }

        private string _status; 
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                RaisePropertyChanged("Status");
            }
        }

        /*
         * Constructor
         */
        public ChatModel()
        {
            ShowPopup = false;
            IsNotListening = true;
            IsConnected = false;
           // WaitForName = true;
            InviteAccepted = false;

            LocalIP = "127.0.0.1";
            RemoteIP = "127.0.0.1";
            LocalPort = 8888;
            RemotePort = 8888;

            UserName = System.Environment.MachineName;

            Clients = new ObservableCollection<Client>();
        }

        /*
         * Start the chat connection 
         */
        public void StartListening()
        {
            IsNotListening = false;
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    //Listener that listens for new incoming connections
                    TcpListener listener = new TcpListener(IPAddress.Parse(_localIP), _localPort);

                    listener.Start();

                    Status = "Started listening for clients.";
                    Console.WriteLine("Started listening for clients.");

                    while (!IsNotListening)
                    {
                        if (listener.Pending())
                        {
                            NewClient = new Client(listener.AcceptTcpClient());
                            NewClient.internalReceivedMessage += ReceivedMessage; //subscribe to event

                            PopupMessage = "New incoming connection from: ";

                            ShowPopup = true;
                            while (ShowPopup) //wait for user to accept/decline new connection
                            {
                                Thread.Sleep(100);
                            }

                            if (!Accept)   //Accept and immediately close
                            {
                                Message msg = new Message(UserName);
                                NewClient.SendString(msg.GetDeclineMessage());
                                NewClient.Disconnect();
                                Status = "Client declined.";
                                Console.WriteLine("Client declined.");
                            }
                            else //Accept incoming connection and create new client instance
                            {
                                Status = "Client accepted";
                                Message msg = new Message(UserName);
                                NewClient.SendString(msg.GetAcceptMessage());
                                AddClient();
                            }
                            
                        }
                        else
                        {
                            Thread.Sleep(500); 
                        }
                    }
                    
                    return;//Exit the thread

                }
                catch (SocketException se)
                {
                    Status = "This socket is already in use";
                    Console.WriteLine("This socket adress is already in use.");
                    AddressBusy();
                    return; //exit thread
                }

            });

        }
        
        /*
         * Close the chat and remove all clients
         */
        public void StopListening()
        {
            Status = "Search stopped";
            Console.WriteLine("Chat stopped.");
            IsNotListening = true;
        }


        public void Invite()
        {
            ThreadPool.QueueUserWorkItem(o =>
             {
                 try
                 {
                     Message msg = new Message(UserName);
                     TcpClient tcpclient = new TcpClient(RemoteIP, RemotePort);
                     NewClient = new Client(tcpclient);

                     NewClient.internalReceivedMessage += ReceivedMessage; //subscribe to event

                     NewClient.SendString(msg.GetNameMessage());

                     Console.WriteLine("Waiting for name...");
                     WaitForUser.WaitOne();

                     if (InviteAccepted)
                     {
                         Status = "Invite accepted.";
                         Console.WriteLine("Invite accepted.");
                         AddClient();
                         
                     }
                     else
                     {
                         Status = "Invite declined";
                         //remove client with sender name
                         Clients.Remove(NewClient); //right now removes all duplicates
                     }

                     Console.WriteLine("Exiting invite.");
                     InviteAccepted = false;
                     return;
                 }
                 catch (SocketException se)
                 {
                     Console.WriteLine("There is nobody listening on the remote address.");
                     Status = "There is nobody listening on the remote address.";
            return;//exit thread
                 }
             });
        }

        public void AddClient()
        {
            NewClient.Name = NewConnectionName;

            //Clients.Add(client); //add client to client list
            
            Application.Current.Dispatcher.Invoke(new Action(() => AddClientToList())); 
            
            SelectedClient = NewClient;

            Message msg = new Message(UserName);
            NewClient.SendString(msg.GetNameMessage());

            Console.WriteLine("New client with id " + _numClients + " connected.");

            _numClients++; //increment the number of clients
        }

        public void AddClientToList()
        {
            var client = Clients.FirstOrDefault(x => x.Name == NewClient.Name);

            if (client == null) //there exists no client with the new name, add a new one
            {
                Status = "New client " + NewClient.Name + " connected";
                Clients.Add(NewClient);
            }
            else //Client with that name already exists, update the TCP listener
            {
                Status = "Client already exist in client list, connecting.";
                Clients.Single(x => x.Name == NewClient.Name).TCP_client = NewClient.TCP_client;
            }
        }



        public void RemoveSelectedClient()
        {
            if (Clients.Any())
            {
                Application.Current.Dispatcher.Invoke(new Action(() => RemoveClientFromList()));
                Clients.Remove(SelectedClient);
            }
            else
            {
                Status = "There are no clients connected";
            }
        }

        public void RemoveClientFromList()
        {
            Clients.Remove(SelectedClient);
        }

        public void DisconnectSelectedClient()
        {
            if (Clients.Any())
            {
                Clients.Single(i => i.Name == SelectedClient.Name).Disconnect(); //look for exception here
            }
            else
            {
                Status = "There are no clients to disconnect";
            }
        }

        public void ConnectSelectedClient()
        {
            if (Clients.Any())
            {
                Clients.Single(i => i.Name == SelectedClient.Name).Connect(); //look for exception here
            }
            else
            {
                Status = "There are no clients to connect";
            }
        }

        public void SendToSelectedClient(string msg)
        {

            if (Clients.Single(i => i.Name == SelectedClient.Name).IsConnected)
            {
                Status = "Message sent to " + SelectedClient.Name; 
                Clients.Single(i => i.Name == SelectedClient.Name).SendMessage(msg, UserName);
            }
            else
            {
                Status = "Can't send, the client is not connected";
            }
        }

        
        /*
         * Received message event
         */
        private void ReceivedMessage(object sender, string str)
        {

            try
            {
                string[] splitStr = str.Split('|');

                if (splitStr.Length > 1)
                {
                    if (splitStr[0].Equals("N"))
                    {
                        Console.WriteLine("Received name");
                        NewConnectionName = splitStr[1];
                    }
                    else if (splitStr[0].Equals("M"))
                    {
                        LastReceivedMessage = (sender as Client).Name + ": " + splitStr[1];
                    }
                    else if (splitStr[0].Equals("A"))
                    {
                        Console.WriteLine("Received accept");
                        NewConnectionName = splitStr[1];
                        InviteAccepted = true;
                        WaitForUser.Set();
                    }
                    else if (splitStr[0].Equals("D"))
                    {
                        Console.WriteLine("Received decline");
                        InviteAccepted = false;
                        WaitForUser.Set();
                    }
                    else if (splitStr[0].Equals("d"))
                    {
                        Console.WriteLine("Received disconnect");
                        //MessageBox.Show("User " + splitStr[1] + " disconnected.");
                        Status = "User " + splitStr[1] + " disconnected.";
                        Clients.Single(x => x.Name == splitStr[1]).Disconnect(); //look for exception here
                        //RemoveSelectedClient();                        
                    }
                }
                else
                {
                    LastReceivedMessage = str;
                }
            }
            catch (ArgumentOutOfRangeException aoe)
            {
                Console.WriteLine("Tried to remove item in list that does not exist");
                Console.WriteLine(aoe);
            }
            
        }
        
    }
}

