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
    /// <summary>
    /// Chat model containing most of the important functions and properties of the chat
    /// </summary>
    public class ChatModel : BaseViewModel
    {
        // =====================================================================
        // Properties
        // =====================================================================
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

        /// <summary>
        /// If the client has been accepted or not
        /// </summary>
        /// <value></value>
        public bool Accept { get; set; }
        /// <summary>
        /// if we are not listening for new connections
        /// </summary>
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

        /// <summary>
        /// List with all clients
        /// This collection is bound to the GUI
        /// </summary>
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

        /// <summary>
        /// The client currently selected in the GUI
        /// TODO: this should probaly be a pointer instead.
        /// </summary>
        private Client _selectedClient;
        public Client SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                _selectedClient = value;
                RaisePropertyChanged("SelectedClient");
            }
        }

        /// <summary>
        /// When a new client is connected it is instansiated here
        /// If the connection is accepted this will be added to the clients list, otherwise ignored
        /// </summary>
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

        /// <summary>
        /// The last message received
        /// TODO: change this!
        /// </summary>
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
        /// <summary>
        /// Event used by the view model
        /// </summary>
        public delegate void AddresAlreadyInUse();
        public event AddresAlreadyInUse AddressBusy;
        /// <summary>
        /// If we have to wait for an accept/decline message
        /// </summary>
        /// <returns></returns>
        public AutoResetEvent WaitForUser = new AutoResetEvent(false);
        /// <summary>
        /// Is true if the invite is accepted
        /// </summary>
        /// <value></value>
        public bool InviteAccepted { get; set; }

        /// <summary>
        /// If the popup window is shown or not
        /// </summary>
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
        /// <summary>
        /// The message shown in the popup window
        /// </summary>
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
        /// <summary>
        /// The name of a new incoming connection
        /// </summary>
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
        /// <summary>
        /// A status message bound to the GUI
        /// </summary>
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

        // =============================================================================
        // Constructors
        // =============================================================================
        public ChatModel()
        {
            ShowPopup = false;
            IsNotListening = true;
            IsConnected = false;
            InviteAccepted = false;

            LocalIP = "127.0.0.1";
            RemoteIP = "127.0.0.1";
            LocalPort = 8888;
            RemotePort = 8888;

            UserName = System.Environment.MachineName;

            Clients = new ObservableCollection<Client>();
        }

        // =====================================================================
        // Member functions
        // =====================================================================

        /// <summary>
        /// Listen for incoming connection invites
        /// This function is run in its own thread
        /// If a new connection is received, a popup will let you accept or deny
        /// </summary>
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

        /// <summary>
        /// Invite the specified remote connection
        /// If the invite is accepted, add it to Clients
        /// </summary>
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
        /// <summary>
        /// Add a new client
        /// The function runs AddClientToList from the main thread because Clients can't be modified by another thread
        /// </summary>
        public void AddClient()
        {
            NewClient.Name = NewConnectionName;

            //Clients.Add(client); //add client to client list

            Application.Current.Dispatcher.Invoke(new Action(() => AddClientToList()));

            SelectedClient = NewClient;

            Message msg = new Message(UserName);
            NewClient.SendString(msg.GetNameMessage());

        }

        /// <summary>
        /// Add the NewClient client to the client list
        /// If a client with the same name already exists, connect to it instead with the new tcpClient
        /// </summary>
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

        /// <summary>
        /// Remove the selected client from the client list
        /// This will also disconnect it 
        /// </summary>
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

        /// <summary>
        /// Helper function run by RemoveSelectedClient
        /// This function is run from the main thread since it has to modify Clients
        /// </summary>
        public void RemoveClientFromList()
        {
            Clients.Remove(SelectedClient);
        }

        /// <summary>
        /// Disconnect from the selected client
        /// </summary>
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

        /// <summary>
        /// Connect to the selected client
        /// </summary>
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
        /// <summary>
        /// Send a string to the selected client
        /// </summary>
        /// <param name="msg">string to send</param>
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

        /// <summary>
        /// Function is called when a message has been received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="str">The received string</param>
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
                        Status = "User " + splitStr[1] + " disconnected.";
                        Clients.Single(x => x.Name == splitStr[1]).Disconnect(); //look for exception here             
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

